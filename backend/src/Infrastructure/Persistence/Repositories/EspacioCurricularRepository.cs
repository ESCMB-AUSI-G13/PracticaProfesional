using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class EspacioCurricularRepository(AppDbContext db) : IEspacioCurricularRepository
{
    public async Task<IEnumerable<EspacioCurricular>> ListarAsync(CancellationToken cancellationToken = default)
        => await db.EspaciosCurriculares
            .Include(ec => ec.Materia).ThenInclude(m => m.Carrera)
            .Include(ec => ec.Docente).ThenInclude(d => d.Usuario)
            .Include(ec => ec.Curso)
            .OrderBy(ec => ec.Curso.AnioLectivo).ThenBy(ec => ec.Curso.Comision).ThenBy(ec => ec.Materia.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<EspacioCurricular>> ListarPorDocenteIdAsync(int docenteId, CancellationToken cancellationToken = default)
        => await db.EspaciosCurriculares
            .Include(ec => ec.Materia)
            .Include(ec => ec.Curso)
            .Where(ec => ec.DocenteId == docenteId)
            .OrderBy(ec => ec.Curso.AnioLectivo).ThenBy(ec => ec.Curso.Comision).ThenBy(ec => ec.Materia.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<EspacioCurricular?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.EspaciosCurriculares
            .Include(ec => ec.Materia)
            .Include(ec => ec.Docente).ThenInclude(d => d.Usuario)
            .Include(ec => ec.Curso)
            .FirstOrDefaultAsync(ec => ec.Id == id, cancellationToken);

    public async Task<bool> ExisteAsync(int materiaId, int docenteId, int cursoId, CancellationToken cancellationToken = default)
        => await db.EspaciosCurriculares
            .AnyAsync(ec => ec.MateriaId == materiaId && ec.DocenteId == docenteId && ec.CursoId == cursoId, cancellationToken);

    public async Task AgregarAsync(EspacioCurricular ec, CancellationToken cancellationToken = default)
    {
        await db.EspaciosCurriculares.AddAsync(ec, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task EliminarAsync(EspacioCurricular ec, CancellationToken cancellationToken = default)
    {
        db.EspaciosCurriculares.Remove(ec);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await db.SaveChangesAsync(cancellationToken);
}
