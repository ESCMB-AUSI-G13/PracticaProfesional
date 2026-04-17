using PracticaProfesional.Application.Calificaciones.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;
using PracticaProfesional.Domain.ValueObjects;

namespace PracticaProfesional.Application.Calificaciones;

/// <summary>
/// Rectifica una nota ya cargada (estado Aprobada o Desaprobada).
/// Registra el cambio con ValorAnterior / ValorNuevo y el motivo de corrección
/// en AuditoriaLogs de forma inmutable (CU-06).
/// </summary>
public class RectificarNotaExamenUseCase(
    IInscripcionExamenRepository inscripcionExamenRepository,
    IAuditoriaService auditoria)
{
    public async Task<NotaExamenResultDto> EjecutarAsync(
        RectificarNotaExamenDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Motivo))
            throw new BusinessException("El motivo de rectificación es obligatorio.");

        // 1. Obtener inscripción con datos completos
        var inscripcion = await inscripcionExamenRepository
            .ObtenerPorIdAsync(dto.InscripcionExamenId, cancellationToken)
            ?? throw new BusinessException(
                $"No se encontró la inscripción a examen con Id {dto.InscripcionExamenId}.");

        // 2. Solo se puede rectificar si ya tiene nota (Aprobada / Desaprobada)
        if (inscripcion.Estado != EstadoInscripcion.Aprobada &&
            inscripcion.Estado != EstadoInscripcion.Desaprobada)
            throw new BusinessException(
                $"No se puede rectificar: la inscripción está en estado '{inscripcion.Estado}'. " +
                "Solo se rectifican notas ya cargadas.");

        // 3. Capturar estado anterior para auditoría
        var valorAnterior = new
        {
            NotaValor = inscripcion.NotaValor,
            Estado    = inscripcion.Estado.ToString()
        };

        // 4. Crear el VO Nota — valida rango 1-10
        var nuevaNota = Nota.Crear(dto.NuevaNota);

        // 5. Aplicar la rectificación en el dominio
        inscripcion.RectificarNota(nuevaNota);

        // 6. Persistir
        await inscripcionExamenRepository.GuardarCambiosAsync(cancellationToken);

        // 7. Auditoría inmutable (CU-06) — incluye motivo en el valor nuevo
        await auditoria.RegistrarAsync(
            entidadTipo: "InscripcionExamen",
            entidadId:   inscripcion.Id.ToString(),
            accion:      "RECTIFICAR_NOTA",
            valorAnterior: valorAnterior,
            valorNuevo: new
            {
                NotaValor = inscripcion.NotaValor,
                Estado    = inscripcion.Estado.ToString(),
                Motivo    = dto.Motivo.Trim()
            },
            cancellationToken: cancellationToken);

        var estudiante = inscripcion.Estudiante;
        var examen     = inscripcion.Examen;

        return new NotaExamenResultDto(
            InscripcionExamenId:      inscripcion.Id,
            EstudianteId:             inscripcion.EstudianteId,
            EstudianteNombreCompleto: $"{estudiante.Usuario.Nombre} {estudiante.Usuario.Apellido}",
            EstudianteLegajo:         estudiante.Usuario.Legajo,
            ExamenId:                 inscripcion.ExamenId,
            TipoExamen:               examen.TipoExamen.ToString(),
            MateriaNombre:            examen.Materia.Nombre,
            NotaValor:                nuevaNota.Valor,
            EsAprobado:               nuevaNota.EsAprobado,
            Estado:                   inscripcion.Estado.ToString());
    }
}
