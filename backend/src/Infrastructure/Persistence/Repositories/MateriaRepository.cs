using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Materias.DTOs;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class MateriaRepository(AppDbContext context) : IMateriaRepository
{
    public async Task<IEnumerable<MateriaDto>> ListarAsync(CancellationToken cancellationToken = default)
        => await context.Materias
            .AsNoTracking()
            .OrderBy(m => m.Carrera.Nombre).ThenBy(m => m.Nombre)
            .Select(m => new MateriaDto(m.Id, m.Codigo, m.Nombre, m.CarreraId, m.Carrera.Nombre, m.Anio))
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<MateriaDto>> ListarPorCarreraIdAsync(int carreraId, CancellationToken cancellationToken = default)
        => await context.Materias
            .AsNoTracking()
            .Where(m => m.CarreraId == carreraId)
            .OrderBy(m => m.Nombre)
            .Select(m => new MateriaDto(m.Id, m.Codigo, m.Nombre, m.CarreraId, m.Carrera.Nombre, m.Anio))
            .ToListAsync(cancellationToken);

    public async Task<Materia?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Materias
            .Include(m => m.Carrera)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<bool> ExistePorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => await context.Materias.AnyAsync(m => m.Codigo == codigo.ToUpperInvariant(), cancellationToken);

    public async Task<bool> ExistePorCodigoExcluyendoAsync(string codigo, int excludeId, CancellationToken cancellationToken = default)
        => await context.Materias.AnyAsync(m => m.Codigo == codigo.ToUpperInvariant() && m.Id != excludeId, cancellationToken);

    public async Task<int> ObtenerSiguienteNumeroAsync(CancellationToken cancellationToken = default)
    {
        var codigos = await context.Materias
            .Select(m => m.Codigo)
            .ToListAsync(cancellationToken);

        var maxNumero = codigos
            .Where(c => c.StartsWith("MAT-") && int.TryParse(c[4..], out _))
            .Select(c => int.Parse(c[4..]))
            .DefaultIfEmpty(0)
            .Max();

        return maxNumero + 1;
    }

    public async Task AgregarAsync(Materia materia, CancellationToken cancellationToken = default)
    {
        await context.Materias.AddAsync(materia, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);

    public async Task<int?> ObtenerCarreraIdAsync(int materiaId, CancellationToken cancellationToken = default)
        => await context.Materias.Where(m => m.Id == materiaId).Select(m => (int?)m.CarreraId).FirstOrDefaultAsync(cancellationToken);

    public async Task<int> ContarPorCarreraIdAsync(int carreraId, CancellationToken cancellationToken = default)
        => await context.Materias.CountAsync(m => m.CarreraId == carreraId, cancellationToken);

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var materia = await context.Materias.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró la materia con Id {id}.");
        context.Materias.Remove(materia);
        await context.SaveChangesAsync(cancellationToken);
    }
}
