using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Correlatividades;

public class ListarCorrelativiadadesUseCase(ICorrelativiadadRepository correlativiadadRepository)
{
    public async Task<IEnumerable<CorrelativiadadDto>> EjecutarAsync(
        int materiaDestinoId,
        CancellationToken cancellationToken = default)
    {
        var correlatividades = await correlativiadadRepository
            .ListarPorMateriaDestinoAsync(materiaDestinoId, cancellationToken);

        return correlatividades.Select(c => new CorrelativiadadDto(
            c.Id,
            c.MateriaDestinoId,
            c.MateriaRequisitoId,
            c.MateriaRequisito?.Nombre ?? $"Materia {c.MateriaRequisitoId}",
            c.TipoRequerimiento,
            c.CondicionAcademica.ToString()));
    }
}
