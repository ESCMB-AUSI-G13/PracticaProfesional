using PracticaProfesional.Application.Inscripciones.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Inscripciones;

public class ObtenerComprobanteInscripcionUseCase(IInscripcionMateriaRepository repository)
{
    public async Task<ComprobanteInscripcionMateriaDto> EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var inscripcion = await repository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró la inscripción con Id {id}.");

        return new ComprobanteInscripcionMateriaDto(
            inscripcion.Id,
            $"{inscripcion.Estudiante.Usuario.Apellido}, {inscripcion.Estudiante.Usuario.Nombre}",
            inscripcion.Estudiante.Usuario.DNI,
            inscripcion.Estudiante.Usuario.Legajo,
            inscripcion.Materia.Codigo,
            inscripcion.Materia.Nombre,
            inscripcion.Materia.Carrera?.Nombre ?? string.Empty,
            inscripcion.Curso.AnioLectivo,
            inscripcion.Curso.Comision,
            inscripcion.Estado.ToString(),
            inscripcion.FechaInscripcion,
            DateTime.UtcNow
        );
    }
}
