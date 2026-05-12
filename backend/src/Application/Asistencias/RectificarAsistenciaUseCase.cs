using PracticaProfesional.Application.Asistencias.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Asistencias;

public class RectificarAsistenciaUseCase(
    IEspacioCurricularRepository espacioRepository,
    IAsistenciaRepository asistenciaRepository,
    IAuditoriaService auditoria)
{
    public async Task EjecutarAsync(
        RectificarAsistenciaCommand command,
        CancellationToken cancellationToken = default)
    {
        var espacio = await espacioRepository.ObtenerPorIdAsync(command.EspacioCurricularId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el espacio curricular con Id {command.EspacioCurricularId}.", 404);

        var asistencias = await asistenciaRepository.ObtenerPorEspacioYFechaAsync(
            espacio.CursoId, espacio.MateriaId, command.Fecha.Date, cancellationToken);

        if (!asistencias.Any())
            throw new BusinessException(
                $"No existe registro de asistencia para la fecha {command.Fecha:dd/MM/yyyy}.", 404);

        var mapa = asistencias.ToDictionary(a => a.Id);

        foreach (var cambio in command.Cambios)
        {
            if (!mapa.TryGetValue(cambio.AsistenciaId, out var asistencia))
                throw new BusinessException($"No se encontró la asistencia con Id {cambio.AsistenciaId}.");

            if (!Enum.TryParse<EstadoAsistencia>(cambio.NuevoEstado, out var nuevoEstado))
                throw new BusinessException($"Estado inválido: '{cambio.NuevoEstado}'.");

            var estadoAnterior = asistencia.Estado.ToString();
            var motivoAnterior = asistencia.Motivo;

            asistencia.Rectificar(nuevoEstado, cambio.Motivo?.Trim());

            await auditoria.RegistrarAsync(
                entidadTipo: "Asistencia",
                entidadId: asistencia.Id.ToString(),
                accion: "RECTIFICAR",
                valorAnterior: new { Estado = estadoAnterior, Motivo = motivoAnterior },
                valorNuevo: new { Estado = cambio.NuevoEstado, Motivo = cambio.Motivo },
                cancellationToken: cancellationToken);
        }

        await asistenciaRepository.GuardarCambiosAsync(cancellationToken);
    }
}
