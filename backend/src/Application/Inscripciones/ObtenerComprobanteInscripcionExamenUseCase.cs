using PracticaProfesional.Application.Inscripciones.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Inscripciones;

public class ObtenerComprobanteInscripcionExamenUseCase(
    IInscripcionExamenRepository repository,
    IEstudianteRepository estudianteRepository)
{
    public async Task<ComprobanteInscripcionExamenDto> EjecutarAsync(
        int id, int usuarioId, bool esDireccion, CancellationToken cancellationToken = default)
    {
        var inscripcion = await repository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró la inscripción a examen con Id {id}.");

        if (!esDireccion)
        {
            var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
            if (estudiante is null || estudiante.Id != inscripcion.EstudianteId)
                throw new BusinessException("No tenés permiso para ver este comprobante.", 403);
        }

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
