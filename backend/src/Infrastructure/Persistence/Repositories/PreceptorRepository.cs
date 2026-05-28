using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Preceptores.DTOs;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class PreceptorRepository(AppDbContext context) : IPreceptorRepository
{
    public async Task<Preceptor?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default)
        => await context.Preceptores
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId, cancellationToken);

    public async Task<IEnumerable<PreceptorDto>> ListarAsync(CancellationToken cancellationToken = default)
        => await context.Preceptores
            .AsNoTracking()
            .OrderBy(p => p.Usuario.Apellido)
            .ThenBy(p => p.Usuario.Nombre)
            .Select(p => new PreceptorDto(
                p.Id, p.UsuarioId, p.Usuario.DNI, p.Usuario.Legajo, p.Usuario.Email,
                p.Usuario.Nombre, p.Usuario.Apellido, p.Telefono, p.Turno,
                p.Usuario.Activo, p.Usuario.FechaCreacion))
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(Preceptor preceptor, CancellationToken cancellationToken = default)
    {
        await context.Preceptores.AddAsync(preceptor, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
