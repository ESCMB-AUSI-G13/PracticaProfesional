using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Docentes.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class DocenteRepository(AppDbContext context) : IDocenteRepository
{
    public async Task<Docente?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default)
        => await context.Docentes
            .Include(d => d.Usuario)
            .FirstOrDefaultAsync(d => d.UsuarioId == usuarioId, cancellationToken);

    public async Task<IEnumerable<DocenteDto>> ListarAsync(CancellationToken cancellationToken = default)
        => await context.Docentes
            .AsNoTracking()
            .OrderBy(d => d.Usuario.Apellido)
            .ThenBy(d => d.Usuario.Nombre)
            .Select(d => new DocenteDto(
                d.Id, d.UsuarioId, d.Usuario.DNI, d.Usuario.Legajo, d.Usuario.Email,
                d.Usuario.Nombre, d.Usuario.Apellido, d.Telefono, d.Categoria,
                d.Usuario.Activo, d.Usuario.FechaCreacion))
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(Docente docente, CancellationToken cancellationToken = default)
    {
        await context.Docentes.AddAsync(docente, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
