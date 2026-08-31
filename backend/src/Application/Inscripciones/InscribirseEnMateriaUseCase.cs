using System.Data;
using PracticaProfesional.Application.Inscripciones.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Inscripciones;

/// <summary>
/// CU-22: Inscripción autogestionada a materia con validación automática de correlatividades.
/// Regla: para CURSAR una materia el estudiante debe estar Regularizado en sus correlativas.
/// </summary>
public class InscribirseEnMateriaUseCase(
    IEstudianteRepository estudianteRepository,
    IInscripcionMateriaRepository inscripcionMateriaRepository,
    ICorrelativiadadRepository correlativiadadRepository,
    IHistorialAcademicoRepository historialRepository,
    ICalendarioAcademicoRepository calendarioRepository,
    IEspacioCurricularRepository espacioCurricularRepository,
    IAuditoriaService auditoria,
    IUnitOfWork unitOfWork)
{
    public async Task<InscripcionMateriaResultDto> EjecutarAsync(
        InscribirseEnMateriaDto dto,
        CancellationToken cancellationToken = default)
    {
        // 1. Verificar que el estudiante existe
        var estudiante = await estudianteRepository.ObtenerPorIdAsync(dto.EstudianteId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el estudiante con Id {dto.EstudianteId}.");

        // 2. Verificar que el estudiante no está en estado terminal
        if (estudiante.Condicion == CondicionEstudiante.Egresado)
            throw new BusinessException("El estudiante ya egresó y no puede inscribirse a materias.");

        if (estudiante.Condicion == CondicionEstudiante.Desertor)
            throw new BusinessException("El estudiante está en condición de desertor. Debe re-inscribirse primero.");

        // 3. Validar período de inscripción (CU-47) — acotado a esta materia/curso si el
        // evento de calendario lo especifica (ver CHECKLIST.md, Tier 5 #19).
        if (!await calendarioRepository.EstaEnPeriodoAsync(
                TipoEvento.InscripcionMateria, DateTime.Today, dto.MateriaId, dto.CursoId, cancellationToken))
            throw new BusinessException("Inscripción fuera del período habilitado según el Calendario Académico.");

        // 4. Verificar que no existe ya una inscripción activa para esa materia
        if (await inscripcionMateriaRepository.ExisteInscripcionActivaAsync(dto.EstudianteId, dto.MateriaId, cancellationToken))
            throw new BusinessException("El estudiante ya tiene una inscripción activa en esta materia.");

        // 5. Validar correlatividades para CURSAR
        await ValidarCorrelativiadadesParaCursarAsync(dto.EstudianteId, dto.MateriaId, cancellationToken);

        // 5b. Validar que la materia efectivamente se dicte en el curso elegido
        var espacios = await espacioCurricularRepository.ListarPorCursoYMateriaAsync(dto.CursoId, dto.MateriaId, cancellationToken);
        if (!espacios.Any())
            throw new BusinessException("La materia seleccionada no se dicta en el curso elegido.");

        // 5c. El curso debe estar Activo (no Cerrado ni Suspendido)
        var curso = espacios.First().Curso;
        if (curso.Estado != EstadoCurso.Activo)
            throw new BusinessException($"No se puede inscribir: el curso se encuentra en estado '{curso.Estado}'.");

        // 5d-7. Validar cupo + crear inscripción + auditar, todo dentro de una transacción
        // Serializable: el conteo de "activas en curso" y el insert deben verse como una sola
        // operación atómica, o dos inscripciones concurrentes pueden leer el mismo conteo antes
        // de que ninguna haga commit y ambas pasar un cupo ya lleno (race condition confirmada
        // en vivo — ver CHECKLIST.md, Tier 2 #5).
        InscripcionMateria inscripcion = null!;
        await unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var activasEnCurso = await inscripcionMateriaRepository
                .ListarActivasPorCursoYMateriaAsync(dto.CursoId, dto.MateriaId, cancellationToken);
            if (activasEnCurso.Count() >= curso.Cupo)
                throw new BusinessException("No hay cupo disponible en este curso.", 409);

            inscripcion = InscripcionMateria.Crear(dto.EstudianteId, dto.MateriaId, dto.CursoId);
            await inscripcionMateriaRepository.AgregarAsync(inscripcion, cancellationToken);

            await auditoria.RegistrarAsync(
                "InscripcionMateria",
                inscripcion.Id.ToString(),
                "CREAR",
                valorAnterior: null,
                valorNuevo: new { inscripcion.EstudianteId, inscripcion.MateriaId, inscripcion.CursoId, Estado = inscripcion.Estado.ToString() },
                cancellationToken);
        }, IsolationLevel.Serializable, cancellationToken);

        // inscripcion.Materia nunca viene cargada (InscripcionMateria.Crear no trae la
        // navegación) — antes esto dejaba materiaNombre siempre vacío en la respuesta. El
        // espacio curricular ya resuelto en el paso 5b sí tiene la navegación (ver CHECKLIST.md,
        // Tier 7 #36).
        return new InscripcionMateriaResultDto(
            inscripcion.Id,
            inscripcion.EstudianteId,
            inscripcion.MateriaId,
            espacios.First().Materia?.Nombre ?? string.Empty,
            inscripcion.CursoId,
            inscripcion.Estado.ToString(),
            inscripcion.FechaInscripcion);
    }

    // ── Validación de correlatividades ───────────────────────────────────────────

    private async Task ValidarCorrelativiadadesParaCursarAsync(
        int estudianteId,
        int materiaId,
        CancellationToken cancellationToken)
    {
        var correlatividades = (await correlativiadadRepository.ObtenerParaCursarAsync(materiaId, cancellationToken)).ToList();

        // Un solo viaje a la base para todo el historial del estudiante en vez de una consulta
        // EstaRegularizado/EstaAprobado por cada correlatividad (N+1) — con varias correlativas
        // (régimen Res. 0013) esto era la parte más lenta de inscribirse.
        var historial = correlatividades.Count > 0
            ? (await historialRepository.ObtenerPorEstudianteAsync(estudianteId, cancellationToken)).ToList()
            : [];

        var requisitosIncumplidos = new List<string>();

        foreach (var correlatividad in correlatividades)
        {
            var materiaRequisito = correlatividad.MateriaRequisito;
            var nombreRequisito = materiaRequisito?.Nombre ?? $"Materia Id {correlatividad.MateriaRequisitoId}";

            bool cumple = correlatividad.CondicionAcademica switch
            {
                // Para cursar: el requisito es tener la materia regularizada
                CondicionAcademica.Regularizado =>
                    historial.Any(h => h.MateriaId == correlatividad.MateriaRequisitoId &&
                        (h.Condicion == CondicionEstudiante.Regular || h.Condicion == CondicionEstudiante.Promocional)),

                // Para cursar con requisito aprobado: debe haberla aprobado (caso menos común)
                CondicionAcademica.Aprobado =>
                    historial.Any(h => h.MateriaId == correlatividad.MateriaRequisitoId &&
                        h.NotaFinal.HasValue && h.NotaFinal >= 4),

                _ => false
            };

            if (!cumple)
            {
                var condicionRequerida = correlatividad.CondicionAcademica == CondicionAcademica.Regularizado
                    ? "regularizada"
                    : "aprobada";
                requisitosIncumplidos.Add($"'{nombreRequisito}' ({condicionRequerida})");
            }
        }

        if (requisitosIncumplidos.Count > 0)
        {
            var detalle = string.Join(", ", requisitosIncumplidos);
            throw new BusinessException(
                $"No se cumplen las correlatividades para inscribirse. " +
                $"Debe tener las siguientes materias: {detalle}.");
        }
    }
}
