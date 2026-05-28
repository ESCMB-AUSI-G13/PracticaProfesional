using PracticaProfesional.Application.Inscripciones.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Inscripciones;

public class ListarInscripcionesUseCase(IInscripcionMateriaRepository repository)
{
    public Task<IEnumerable<InscripcionMateriaListadoDto>> EjecutarAsync(CancellationToken cancellationToken = default)
        => repository.ListarAsync(cancellationToken);
}
