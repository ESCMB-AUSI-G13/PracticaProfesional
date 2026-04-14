using PracticaProfesional.Application.Auditoria.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Auditoria;

public class ListarAuditoriaLogsUseCase(IAuditoriaLogRepository repository)
{
    public async Task<PaginadoDto<AuditoriaLogDto>> EjecutarAsync(
        AuditoriaLogFiltroDto filtro,
        CancellationToken cancellationToken = default)
    {
        var (items, total) = await repository.ListarAsync(filtro, cancellationToken);

        var dtos = items.Select(log => new AuditoriaLogDto(
            log.Id,
            log.EntidadTipo,
            log.EntidadId,
            log.Accion,
            log.EjecutorId,
            log.EjecutorEmail,
            log.ValorAnterior,
            log.ValorNuevo,
            log.Timestamp
        ));

        var totalPaginas = (int)Math.Ceiling((double)total / filtro.TamanoPagina);

        return new PaginadoDto<AuditoriaLogDto>(dtos, total, filtro.Pagina, filtro.TamanoPagina, totalPaginas);
    }
}
