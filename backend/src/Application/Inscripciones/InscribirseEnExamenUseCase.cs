using PracticaProfesional.Application.Inscripciones.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Inscripciones;

/// <summary>
/// CU-33: Inscripción a examen con validación automática de correlatividades para RENDIR.
/// Regla: para rendir un final el estudiante debe tener Aprobado sus correlativas de rendir.
/// </summary>
public class InscribirseEnExamenUseCase(
    IEstudianteRepository estudianteRepository,
    IExamenRepository examenRepository,
    IInscripcionExamenRepository inscripcionExamenRepository,
    IInscripcionMateriaRepository inscripcionMateriaRepository,
    ICorrelativiadadRepository correlativiadadRepository,
    IHistorialAcademicoRepository historialRepository,
    ICalendarioAcademicoRepository calendarioRepository,
    IAuditoriaService auditoria)
{
    public async Task<InscripcionExamenResultDto> EjecutarAsync(
        InscribirseEnExamenDto dto, CancellationToken cancellationToken = default)
    {
        var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(dto.UsuarioId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el perfil de estudiante para el usuario {dto.UsuarioId}.");

        if (estudiante.Condicion == CondicionEstudiante.Egresado)
            throw new BusinessException("El estudiante ya egresó.");
        if (estudiante.Condicion == CondicionEstudiante.Desertor)
            throw new BusinessException("El estudiante está en condición de desertor.");

        // Validar período de inscripción a examen (CU-47)
        if (!await calendarioRepository.EstaEnPeriodoAsync(TipoEvento.InscripcionExamen, DateTime.Today, cancellationToken))
            throw new BusinessException("Inscripción a examen fuera del período habilitado según el Calendario Académico.");

        var examen = await examenRepository.ObtenerPorIdAsync(dto.ExamenId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el examen con Id {dto.ExamenId}.");

        // Para finales: el alumno debe haber cursado la materia (inscripción activa o regularizada)
        if (examen.TipoExamen is TipoExamen.Final)
        {
            var inscriptoEnMateria = await inscripcionMateriaRepository
                .ExisteInscripcionActivaAsync(estudiante.Id, examen.MateriaId, cancellationToken);
            if (!inscriptoEnMateria)
                throw new BusinessException(
                    "Para inscribirse al examen final debe tener la materia cursada (inscripción activa).");

            await ValidarCorrelativiadadesParaRendirAsync(estudiante.Id, examen.MateriaId, cancellationToken);
        }

        var inscripcion = InscripcionExamen.Crear(estudiante.Id, dto.ExamenId);
        await inscripcionExamenRepository.AgregarAsync(inscripcion, cancellationToken);

        await auditoria.RegistrarAsync("InscripcionExamen", inscripcion.Id.ToString(), "CREAR",
            valorAnterior: null,
            valorNuevo: new { inscripcion.EstudianteId, inscripcion.ExamenId, Estado = inscripcion.Estado.ToString() },
            cancellationToken: cancellationToken);

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
        var correlatividades = await correlativiadadRepository.ObtenerParaRendirAsync(materiaId, cancellationToken);
        var incumplidos = new List<string>();

        foreach (var corr in correlatividades)
        {
            var nombre = corr.MateriaRequisito?.Nombre ?? $"Materia Id {corr.MateriaRequisitoId}";
            var cumple = await historialRepository.EstaAprobadoAsync(estudianteId, corr.MateriaRequisitoId, cancellationToken);
            if (!cumple) incumplidos.Add($"'{nombre}' (aprobada)");
        }

        if (incumplidos.Count > 0)
            throw new BusinessException(
                $"No cumple correlatividades para rendir. Debe tener: {string.Join(", ", incumplidos)}.");
    }
}
