using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.DNI)
                .IsRequired()
                .HasMaxLength(10);
            entity.HasIndex(u => u.DNI).IsUnique();

            entity.Property(u => u.Legajo)
                .IsRequired()
                .HasMaxLength(20);
            entity.HasIndex(u => u.Legajo).IsUnique();

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(150);
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.Apellido)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.PasswordHash)
                .IsRequired();

            entity.Property(u => u.Rol)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(u => u.Activo)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(u => u.FechaCreacion)
                .IsRequired();
        });
    }
}
