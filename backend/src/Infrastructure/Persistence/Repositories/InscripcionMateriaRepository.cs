using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Inscripciones.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class InscripcionMateriaRepository(AppDbContext context) : IInscripcionMateriaRepository
{
    public async Task<IEnumerable<InscripcionMateriaListadoDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var raw = await context.InscripcionesMateria
            .AsNoTracking()
            .OrderByDescending(i => i.FechaInscripcion)
            .Select(i => new {
                i.Id, i.EstudianteId, i.MateriaId, i.CursoId, i.Estado, i.FechaInscripcion,
                EstudianteNombre = i.Estudiante.Usuario.Apellido + ", " + i.Estudiante.Usuario.Nombre,
                EstudianteLegajo = i.Estudiante.Usuario.Legajo,
                CarreraId        = i.Estudiante.CarreraId,
                CarreraNombre    = i.Estudiante.Carrera.Nombre,
                MateriaCodigo    = i.Materia.Codigo,
                MateriaNombre    = i.Materia.Nombre,
                CursoAnio        = i.Curso.Anio,
                CursoAnioLectivo = i.Curso.AnioLectivo,
                CursoComision    = i.Curso.Comision
            })
            .ToListAsync(cancellationToken);

        return raw.Select(i => new InscripcionMateriaListadoDto(
            i.Id, i.EstudianteId, i.EstudianteNombre, i.EstudianteLegajo,
            i.CarreraId, i.CarreraNombre, i.MateriaId, i.MateriaCodigo, i.MateriaNombre,
            i.CursoId, i.CursoAnio, i.CursoAnioLectivo, i.CursoComision,
            i.Estado.ToString(), i.FechaInscripcion));
    }

    public async Task<IEnumerable<InscripcionMateria>> ListarActivasPorMateriaAsync(int materiaId, CancellationToken cancellationToken = default)
        => await context.InscripcionesMateria
            .AsNoTracking()
            .Where(i => i.MateriaId == materiaId && i.Estado == EstadoInscripcion.Activa)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<InscripcionMateria>> ListarActivasPorEstudianteAsync(int estudianteId, CancellationToken cancellationToken = default)
        => await context.InscripcionesMateria
            .AsNoTracking()
            .Include(i => i.Materia)
            .Include(i => i.Curso)
            .Where(i => i.EstudianteId == estudianteId && i.Estado == EstadoInscripcion.Activa)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExisteInscripcionActivaAsync(
        int estudianteId,
        int materiaId,
        CancellationToken cancellationToken = default)
        => await context.InscripcionesMateria
            .AnyAsync(i =>
                i.EstudianteId == estudianteId &&
                i.MateriaId == materiaId &&
                i.Estado == EstadoInscripcion.Activa,
            cancellationToken);

    public async Task<bool> TieneAlgunaInscripcionActivaAsync(
        int estudianteId,
        CancellationToken cancellationToken = default)
        => await context.InscripcionesMateria
            .AnyAsync(i =>
                i.EstudianteId == estudianteId &&
                i.Estado == EstadoInscripcion.Activa,
            cancellationToken);

    public async Task<InscripcionMateria?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.InscripcionesMateria
            .Include(i => i.Materia).ThenInclude(m => m.Carrera)
            .Include(i => i.Estudiante).ThenInclude(e => e.Usuario)
            .Include(i => i.Curso)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task AgregarAsync(InscripcionMateria inscripcion, CancellationToken cancellationToken = default)
    {
        await context.InscripcionesMateria.AddAsync(inscripcion, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);

    public Task<bool> ExistePorMateriaIdAsync(int materiaId, CancellationToken cancellationToken = default)
        => context.InscripcionesMateria.AnyAsync(i => i.MateriaId == materiaId, cancellationToken);

    public async Task<IEnumerable<InscripcionMateria>> ListarActivasPorCursoYMateriaAsync(
        int cursoId,
        int materiaId,
        CancellationToken cancellationToken = default)
        => await context.InscripcionesMateria
            .AsNoTracking()
            .Include(i => i.Estudiante).ThenInclude(e => e.Usuario)
            .Where(i => i.CursoId == cursoId && i.MateriaId == materiaId && i.Estado == EstadoInscripcion.Activa)
            .OrderBy(i => i.Estudiante.Usuario.Apellido).ThenBy(i => i.Estudiante.Usuario.Nombre)
            .ToListAsync(cancellationToken);
}
