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
    IAuditoriaService auditoria)
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

        // 3. Verificar que no existe ya una inscripción activa para esa materia
        if (await inscripcionMateriaRepository.ExisteInscripcionActivaAsync(dto.EstudianteId, dto.MateriaId, cancellationToken))
            throw new BusinessException("El estudiante ya tiene una inscripción activa en esta materia.");

        // 4. Validar correlatividades para CURSAR
        await ValidarCorrelativiadadesParaCursarAsync(dto.EstudianteId, dto.MateriaId, cancellationToken);

        // 5. Crear la inscripción
        var inscripcion = InscripcionMateria.Crear(dto.EstudianteId, dto.MateriaId, dto.CursoId);
        await inscripcionMateriaRepository.AgregarAsync(inscripcion, cancellationToken);

        // 6. Auditoría (CU-06)
        await auditoria.RegistrarAsync(
            "InscripcionMateria",
            inscripcion.Id.ToString(),
            "CREAR",
            valorAnterior: null,
            valorNuevo: new { inscripcion.EstudianteId, inscripcion.MateriaId, inscripcion.CursoId, Estado = inscripcion.Estado.ToString() },
            cancellationToken);

        return new InscripcionMateriaResultDto(
            inscripcion.Id,
            inscripcion.EstudianteId,
            inscripcion.MateriaId,
            inscripcion.Materia?.Nombre ?? string.Empty,
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
        var correlatividades = await correlativiadadRepository.ObtenerParaCursarAsync(materiaId, cancellationToken);

        var requisitosIncumplidos = new List<string>();

        foreach (var correlatividad in correlatividades)
        {
            var materiaRequisito = correlatividad.MateriaRequisito;
            var nombreRequisito = materiaRequisito?.Nombre ?? $"Materia Id {correlatividad.MateriaRequisitoId}";

            bool cumple = correlatividad.CondicionAcademica switch
            {
                // Para cursar: el requisito es tener la materia regularizada
                CondicionAcademica.Regularizado =>
                    await historialRepository.EstaRegularizadoAsync(estudianteId, correlatividad.MateriaRequisitoId, cancellationToken),

                // Para cursar con requisito aprobado: debe haberla aprobado (caso menos común)
                CondicionAcademica.Aprobado =>
                    await historialRepository.EstaAprobadoAsync(estudianteId, correlatividad.MateriaRequisitoId, cancellationToken),

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
