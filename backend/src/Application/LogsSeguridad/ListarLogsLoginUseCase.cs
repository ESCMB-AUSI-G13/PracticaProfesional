using PracticaProfesional.Application.Auditoria.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.LogsSeguridad.DTOs;

namespace PracticaProfesional.Application.LogsSeguridad;

public class ListarLogsLoginUseCase(ILogSeguridadRepository repository)
{
    public async Task<PaginadoDto<LogSeguridadDto>> EjecutarAsync(
        LogSeguridadFiltroDto filtro,
        CancellationToken cancellationToken = default)
    {
        var (items, total) = await repository.ListarAsync(
            filtro.Email,
            filtro.SoloFallidos,
            filtro.FechaDesde,
            filtro.FechaHasta,
            filtro.Pagina,
            filtro.TamanoPagina,
            cancellationToken);

        var dtos = items.Select(l => new LogSeguridadDto(
            l.Id, l.Email, l.Exitoso, l.MotivoFallo,
            l.IpOrigen, l.UserAgent, l.Timestamp));

        var totalPaginas = (int)Math.Ceiling((double)total / filtro.TamanoPagina);

        return new PaginadoDto<LogSeguridadDto>(dtos, total, filtro.Pagina, filtro.TamanoPagina, totalPaginas);
    }
}
