using PracticaProfesional.Application.Calificaciones.DTOs;
using PracticaProfesional.Application.EstadoAcademico;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;
using PracticaProfesional.Domain.ValueObjects;

namespace PracticaProfesional.Application.Calificaciones;

/// <summary>
/// CU — Carga de calificaciones (Rol: Docente).
/// El docente registra la nota obtenida por un estudiante en un examen
/// (parcial escrito/oral, recuperatorio, final escrito/oral).
///
/// Reglas de negocio:
///   - La inscripción al examen debe estar en estado Activa.
///   - Si ya tiene nota cargada (estado Aprobada/Desaprobada) se rechaza la operación;
///     la rectificación requiere un proceso diferente con control de auditoría.
///   - La nota debe estar en el rango 1-10 (validado por el VO Nota).
///   - Todo cambio queda registrado de forma inmutable en Auditoria (CU-06).
/// </summary>
public class CargarNotaExamenUseCase(
    IInscripcionExamenRepository inscripcionExamenRepository,
    IHistorialAcademicoRepository historialRepository,
    IDocenteRepository docenteRepository,
    IEspacioCurricularRepository espacioCurricularRepository,
    ActualizarEstadoAcademicoUseCase evaluarEstadoAcademico,
    IAuditoriaService auditoria)
{
    public async Task<NotaExamenResultDto> EjecutarAsync(
        CargarNotaExamenDto dto,
        CancellationToken cancellationToken = default)
    {
        // 1. Obtener inscripción con datos del estudiante y del examen
        var inscripcion = await inscripcionExamenRepository
            .ObtenerPorIdAsync(dto.InscripcionExamenId, cancellationToken)
            ?? throw new BusinessException(
                $"No se encontró la inscripción a examen con Id {dto.InscripcionExamenId}.");

        // 1b. Solo el docente a cargo de la materia puede cargar la nota de su examen
        if (!await EsDocenteDeLaMateriaAsync(dto.UsuarioId, inscripcion.Examen.MateriaId, cancellationToken))
            throw new BusinessException("No tenés permiso para cargar notas de esta materia.", 403);

        // 2. Validar que la inscripción esté activa (no ya calificada ni dada de baja)
        if (inscripcion.Estado != EstadoInscripcion.Activa)
            throw new BusinessException(
                $"No se puede cargar la nota: la inscripción se encuentra en estado '{inscripcion.Estado}'. " +
                "Solo se permite cargar nota cuando el estado es Activa.");

        // 3. Capturar valor anterior para auditoría inmutable
        var valorAnterior = new
        {
            NotaValor = inscripcion.NotaValor,
            Estado = inscripcion.Estado.ToString()
        };

        // 4. Crear el Value Object Nota — valida rango 1-10 y redondea a 2 decimales
        var nota = Nota.Crear(dto.Nota);

        // 5. Delegar la lógica de transición de estado a la entidad de dominio
        inscripcion.CargarNota(nota);

        // 6. Persistir los cambios tracked por EF Core
        await inscripcionExamenRepository.GuardarCambiosAsync(cancellationToken);

        // 7. Registrar auditoría inmutable (CU-06)
        await auditoria.RegistrarAsync(
            entidadTipo: "InscripcionExamen",
            entidadId: inscripcion.Id.ToString(),
            accion: "CARGAR_NOTA",
            valorAnterior: valorAnterior,
            valorNuevo: new
            {
                NotaValor = inscripcion.NotaValor,
                Estado = inscripcion.Estado.ToString()
            },
            cancellationToken: cancellationToken);

        var estudiante = inscripcion.Estudiante;
        var examen = inscripcion.Examen;

        // 8. Si es un examen Final aprobado, la nota queda como nota definitiva de la
        // materia en HistorialAcademico, y se evalúa el egreso automático (CU-43).
        if (examen.TipoExamen == TipoExamen.Final && inscripcion.Estado == EstadoInscripcion.Aprobada)
            await RegistrarNotaFinalEnHistorialAsync(inscripcion.EstudianteId, examen.MateriaId, nota.Valor, cancellationToken);

        return new NotaExamenResultDto(
            InscripcionExamenId: inscripcion.Id,
            EstudianteId: inscripcion.EstudianteId,
            EstudianteNombreCompleto: $"{estudiante.Usuario.Nombre} {estudiante.Usuario.Apellido}",
            EstudianteLegajo: estudiante.Usuario.Legajo,
            ExamenId: inscripcion.ExamenId,
            TipoExamen: examen.TipoExamen.ToString(),
            MateriaNombre: examen.Materia.Nombre,
            NotaValor: nota.Valor,
            EsAprobado: nota.EsAprobado,
            Estado: inscripcion.Estado.ToString());
    }

    /// <summary>
    /// Actualiza la nota definitiva de la materia en HistorialAcademico y dispara la
    /// evaluación automática de egreso (CU-43). El registro de HistorialAcademico ya
    /// existe porque InscribirseEnExamenUseCase exige estar Regularizado (cierre de
    /// cursada previo) para poder inscribirse a un examen Final; si por datos legacy no
    /// existiera, se omite en vez de intentar crear un registro sin CursoId válido.
    /// </summary>
    private async Task RegistrarNotaFinalEnHistorialAsync(
        int estudianteId, int materiaId, decimal notaValor, CancellationToken cancellationToken)
    {
        var historiales = await historialRepository.ObtenerPorEstudianteYMateriaAsync(estudianteId, materiaId, cancellationToken);
        var historial = historiales.OrderByDescending(h => h.Id).FirstOrDefault();
        if (historial is null) return;

        historial.RegistrarNotaFinal(notaValor, "Aprobada");
        await historialRepository.GuardarCambiosAsync(cancellationToken);

        await evaluarEstadoAcademico.EvaluarSoloEgresoAsync(estudianteId, materiaId, cancellationToken);
    }

    /// <summary>Mismo criterio que ListarEncuestasDocenteUseCase.EsMateriaDelDocenteAsync.</summary>
    private async Task<bool> EsDocenteDeLaMateriaAsync(int usuarioId, int materiaId, CancellationToken cancellationToken)
    {
        var docente = await docenteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
        if (docente is null) return false;

        var espacios = await espacioCurricularRepository.ListarPorDocenteIdAsync(docente.Id, cancellationToken);
        return espacios.Any(e => e.MateriaId == materiaId);
    }
}
