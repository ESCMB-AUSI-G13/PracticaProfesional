using PracticaProfesional.Application.Inscripciones.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Inscripciones;

public class ListarInscripcionesUseCase(IInscripcionMateriaRepository repository)
{
    public async Task<IEnumerable<InscripcionMateriaListadoDto>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var inscripciones = await repository.ListarAsync(cancellationToken);

        return inscripciones.Select(i => new InscripcionMateriaListadoDto(
            i.Id,
            i.EstudianteId,
            $"{i.Estudiante.Usuario.Apellido}, {i.Estudiante.Usuario.Nombre}",
            i.MateriaId,
            i.Materia?.Codigo ?? string.Empty,
            i.Materia?.Nombre ?? string.Empty,
            i.CursoId,
            i.Curso?.Anio ?? 0,
            i.Curso?.Comision ?? string.Empty,
            i.Estado.ToString(),
            i.FechaInscripcion
        ));
    }
}
