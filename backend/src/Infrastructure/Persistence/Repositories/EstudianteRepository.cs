using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Estudiantes.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class EstudianteRepository(AppDbContext context) : IEstudianteRepository
{
    public async Task<Estudiante?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Estudiantes
            .Include(e => e.Usuario)
            .Include(e => e.Carrera)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<Estudiante?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default)
        => await context.Estudiantes
            .Include(e => e.Usuario)
            .Include(e => e.Carrera)
            .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId, cancellationToken);

    public async Task<IEnumerable<EstudianteDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var raw = await context.Estudiantes
            .AsNoTracking()
            .OrderBy(e => e.Usuario.Apellido).ThenBy(e => e.Usuario.Nombre)
            .Select(e => new {
                e.Id, e.UsuarioId, e.Anio, e.CarreraId, e.Condicion, e.FechaDeIngreso,
                DNI          = e.Usuario.DNI,
                Legajo       = e.Usuario.Legajo,
                Email        = e.Usuario.Email,
                Nombre       = e.Usuario.Nombre,
                Apellido     = e.Usuario.Apellido,
                Activo       = e.Usuario.Activo,
                FechaCreacion = e.Usuario.FechaCreacion,
                CarreraNombre = e.Carrera.Nombre
            })
            .ToListAsync(cancellationToken);

        return raw.Select(e => new EstudianteDto(
            e.Id, e.UsuarioId, e.DNI, e.Legajo, e.Email, e.Nombre, e.Apellido,
            e.Anio, e.CarreraId, e.CarreraNombre, e.Condicion.ToString(),
            e.FechaDeIngreso, e.Activo, e.FechaCreacion));
    }

    public async Task<Estudiante?> ObtenerPorLegajoAsync(string legajo, CancellationToken cancellationToken = default)
        => await context.Estudiantes
            .Include(e => e.Usuario)
            .Include(e => e.Carrera)
            .FirstOrDefaultAsync(e => e.Usuario.Legajo == legajo, cancellationToken);

    public async Task AgregarAsync(Estudiante estudiante, CancellationToken cancellationToken = default)
    {
        await context.Estudiantes.AddAsync(estudiante, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
