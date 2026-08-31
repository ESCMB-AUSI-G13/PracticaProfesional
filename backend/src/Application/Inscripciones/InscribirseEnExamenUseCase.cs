using System.Data;
using PracticaProfesional.Application.Inscripciones.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Inscripciones;

/// <summary>
/// CU-33: Inscripción a examen con validación automática de correlatividades para RENDIR.
/// Regla: para rendir un final el estudiante debe tener Aprobado sus correlativas de rendir.
/// Uso administrativo (Dirección inscribe a un estudiante puntual) — recibe EstudianteId
/// explícito, sin el chequeo de encuesta obligatoria, que es específico del flujo autogestionado
/// (ver <see cref="InscribirseEnExamenAutogestUseCase"/>). Mismo criterio que
/// InscribirseEnMateriaUseCase/InscribirseEnMateriaAutogestUseCase.
/// </summary>
public class InscribirseEnExamenUseCase(
    IEstudianteRepository           estudianteRepository,
    IExamenRepository               examenRepository,
    IInscripcionExamenRepository    inscripcionExamenRepository,
    ICorrelativiadadRepository      correlativiadadRepository,
    IHistorialAcademicoRepository   historialRepository,
    ICalendarioAcademicoRepository  calendarioRepository,
    IAuditoriaService               auditoria,
    IUnitOfWork                     unitOfWork)
{
    public async Task<InscripcionExamenResultDto> EjecutarAsync(
        InscribirseEnExamenDto dto, CancellationToken cancellationToken = default)
    {
        var estudiante = await estudianteRepository.ObtenerPorIdAsync(dto.EstudianteId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el estudiante con Id {dto.EstudianteId}.");

        if (estudiante.Condicion == CondicionEstudiante.Egresado)
            throw new BusinessException("El estudiante ya egresó.");
        if (estudiante.Condicion == CondicionEstudiante.Desertor)
            throw new BusinessException("El estudiante está en condición de desertor.");

        var examen = await examenRepository.ObtenerPorIdAsync(dto.ExamenId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el examen con Id {dto.ExamenId}.");

        // Validar período de inscripción a examen (CU-47) — acotado a la materia del examen si
        // el evento de calendario lo especifica (ver CHECKLIST.md, Tier 5 #19).
        if (!await calendarioRepository.EstaEnPeriodoAsync(
                TipoEvento.InscripcionExamen, DateTime.Today, examen.MateriaId, cancellationToken: cancellationToken))
            throw new BusinessException("Inscripción a examen fuera del período habilitado según el Calendario Académico.");

        // Evitar doble inscripción (reintentos de red, doble click)
        if (await inscripcionExamenRepository.ExisteAsync(estudiante.Id, dto.ExamenId, cancellationToken))
            throw new BusinessException("El estudiante ya está inscripto en este examen.", 409);

        // Validar cupo + crear inscripción + auditar, todo dentro de una transacción
        // Serializable: mismo problema y mismo fix que InscribirseEnMateriaUseCase — el
        // conteo de inscriptos activos y el insert deben verse como una sola operación
        // atómica para que dos inscripciones concurrentes no pasen ambas un cupo ya lleno
        // (ver CHECKLIST.md, Tier 2 #6).
        InscripcionExamen inscripcion = null!;
        await unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var inscriptosActivos = (await inscripcionExamenRepository.ObtenerPorExamenAsync(dto.ExamenId, cancellationToken))
                .Count(i => i.Estado == EstadoInscripcion.Activa);
            if (inscriptosActivos >= examen.Cupo)
                throw new BusinessException("No hay cupo disponible para este examen.", 409);

            // Para finales: el alumno debe estar Regularizado en la materia (cursada cerrada
            // sin perder la regularidad), no simplemente tener una inscripción activa.
            if (examen.TipoExamen is TipoExamen.Final)
            {
                var regularizado = await historialRepository
                    .EstaRegularizadoAsync(estudiante.Id, examen.MateriaId, cancellationToken);
                if (!regularizado)
                    throw new BusinessException(
                        "Para inscribirse al examen final debe tener la materia regularizada.");

                await ValidarCorrelativiadadesParaRendirAsync(estudiante.Id, examen.MateriaId, cancellationToken);
            }

            inscripcion = InscripcionExamen.Crear(estudiante.Id, dto.ExamenId);
            await inscripcionExamenRepository.AgregarAsync(inscripcion, cancellationToken);

            await auditoria.RegistrarAsync("InscripcionExamen", inscripcion.Id.ToString(), "CREAR",
                valorAnterior: null,
                valorNuevo: new { inscripcion.EstudianteId, inscripcion.ExamenId, Estado = inscripcion.Estado.ToString() },
                cancellationToken: cancellationToken);
        }, IsolationLevel.Serializable, cancellationToken);

        var u = estudiante.Usuario;
        return new InscripcionExamenResultDto(
            inscripcion.Id,
            estudiante.Id,
            $"{u.Apellido}, {u.Nombre}",
            examen.Id,
            examen.Materia.Nombre,
            examen.TipoExamen.ToString(),
            examen.FechaExamen,
            inscripcion.Estado.ToString());
    }

    private async Task ValidarCorrelativiadadesParaRendirAsync(
        int estudianteId, int materiaId, CancellationToken cancellationToken)
    {
        var correlatividades = (await correlativiadadRepository.ObtenerParaRendirAsync(materiaId, cancellationToken)).ToList();

        // Mismo fix que InscribirseEnMateriaUseCase: un solo viaje a la base para todo el
        // historial en vez de una consulta EstaAprobado por cada correlatividad (N+1).
        var historial = correlatividades.Count > 0
            ? (await historialRepository.ObtenerPorEstudianteAsync(estudianteId, cancellationToken)).ToList()
            : [];

        var incumplidos = new List<string>();

        foreach (var corr in correlatividades)
        {
            var nombre = corr.MateriaRequisito?.Nombre ?? $"Materia Id {corr.MateriaRequisitoId}";
            var cumple = historial.Any(h => h.MateriaId == corr.MateriaRequisitoId &&
                h.NotaFinal.HasValue && h.NotaFinal >= 4);
            if (!cumple) incumplidos.Add($"'{nombre}' (aprobada)");
        }

        if (incumplidos.Count > 0)
            throw new BusinessException(
                $"No cumple correlatividades para rendir. Debe tener: {string.Join(", ", incumplidos)}.");
    }
}
