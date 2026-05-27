using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Encuestas;

public class ActivarDesactivarEncuestaUseCase(IEncuestaRepository repo)
{
    public async Task EjecutarAsync(int encuestaId, bool activar, CancellationToken ct = default)
    {
        var encuesta = await repo.ObtenerConPreguntasAsync(encuestaId, ct)
            ?? throw new BusinessException($"No se encontró la encuesta {encuestaId}.");

        if (activar) encuesta.Activar();
        else         encuesta.Desactivar();

        await repo.GuardarCambiosAsync(ct);
    }
}
