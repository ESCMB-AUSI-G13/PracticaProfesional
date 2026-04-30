using PracticaProfesional.Application.Inscripciones.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Inscripciones;

public class ObtenerComprobanteInscripcionExamenUseCase(IInscripcionExamenRepository repository)
{
    public async Task<ComprobanteInscripcionExamenDto> EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var inscripcion = await repository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró la inscripción a examen con Id {id}.");

        return new ComprobanteInscripcionExamenDto(
            inscripcion.Id,
            $"{inscripcion.Estudiante.Usuario.Apellido}, {inscripcion.Estudiante.Usuario.Nombre}",
            inscripcion.Estudiante.Usuario.DNI,
            inscripcion.Estudiante.Usuario.Legajo,
            inscripcion.Examen.Materia.Codigo,
            inscripcion.Examen.Materia.Nombre,
            inscripcion.Examen.TipoExamen.ToString(),
            inscripcion.Examen.FechaExamen,
            inscripcion.Examen.Horario,
            inscripcion.Estado.ToString(),
            inscripcion.FechaInscripcion,
            DateTime.UtcNow
        );
    }
}
