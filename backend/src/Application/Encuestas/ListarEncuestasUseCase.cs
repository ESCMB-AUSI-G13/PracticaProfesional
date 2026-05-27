using PracticaProfesional.Application.Encuestas.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Encuestas;

public class ListarEncuestasUseCase(IEncuestaRepository repo)
{
    public async Task<List<EncuestaDto>> EjecutarAsync(CancellationToken ct = default)
    {
        var encuestas = await repo.ListarTodasAsync(ct);
        return encuestas.Select(ToDto).ToList();
    }

    internal static EncuestaDto ToDto(Domain.Entities.Encuesta e) => new(
        e.Id,
        e.Titulo,
        e.Descripcion,
        e.Tipo,
        e.MateriaId,
        e.Materia?.Nombre,
        e.CicloLectivo,
        e.Activa,
        e.FechaCreacion,
        e.Preguntas.Select(p => new PreguntaEncuestaDto(
            p.Id, p.Texto, p.Orden, p.TipoPregunta, p.EsObligatoria
        )).ToList());
}
