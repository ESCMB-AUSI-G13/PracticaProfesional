using PracticaProfesional.Application.Calificaciones.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Calificaciones;

/// <summary>
/// Retorna el listado de inscripciones de un examen para que el docente
/// pueda ver qué alumnos están anotados y cuáles ya tienen nota cargada.
/// </summary>
public class ListarInscripcionesExamenUseCase(
    IInscripcionExamenRepository inscripcionExamenRepository)
{
    public async Task<IEnumerable<InscripcionExamenDto>> EjecutarAsync(
        int examenId,
        CancellationToken cancellationToken = default)
    {
        var inscripciones = await inscripcionExamenRepository
            .ObtenerPorExamenAsync(examenId, cancellationToken);

        return inscripciones.Select(i => new InscripcionExamenDto(
            Id: i.Id,
            EstudianteId: i.EstudianteId,
            EstudianteNombreCompleto: $"{i.Estudiante.Usuario.Nombre} {i.Estudiante.Usuario.Apellido}",
            EstudianteLegajo: i.Estudiante.Usuario.Legajo,
            ExamenId: i.ExamenId,
            TipoExamen: i.Examen.TipoExamen.ToString(),
            MateriaNombre: i.Examen.Materia.Nombre,
            NotaValor: i.NotaValor,
            EsAprobado: i.NotaValor.HasValue ? i.NotaValor >= 4 : null,
            Estado: i.Estado.ToString(),
            FechaInscripcion: i.FechaInscripcion));
    }
}
