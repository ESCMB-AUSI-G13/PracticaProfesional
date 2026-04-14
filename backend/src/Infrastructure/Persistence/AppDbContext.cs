using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<AuditoriaCambioRol> AuditoriaCambiosRol => Set<AuditoriaCambioRol>();
    public DbSet<AuditoriaLog> AuditoriaLogs => Set<AuditoriaLog>();
    public DbSet<Docente> Docentes => Set<Docente>();
    public DbSet<Preceptor> Preceptores => Set<Preceptor>();
    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditoriaLog>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.EntidadTipo).IsRequired().HasMaxLength(50);
            entity.Property(l => l.EntidadId).IsRequired().HasMaxLength(20);
            entity.Property(l => l.Accion).IsRequired().HasMaxLength(20);
            entity.Property(l => l.EjecutorEmail).IsRequired().HasMaxLength(150);
            entity.Property(l => l.EjecutorId).IsRequired(false);
            entity.Property(l => l.ValorAnterior).IsRequired(false);
            entity.Property(l => l.ValorNuevo).IsRequired(false);
            entity.Property(l => l.Timestamp).IsRequired();
            entity.HasIndex(l => l.EntidadTipo);
            entity.HasIndex(l => l.Accion);
            entity.HasIndex(l => l.Timestamp);
        });

        modelBuilder.Entity<AuditoriaCambioRol>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.RolOriginal).IsRequired().HasMaxLength(50);
            entity.Property(a => a.RolVista).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Accion).IsRequired().HasMaxLength(20);
            entity.Property(a => a.Timestamp).IsRequired();
            entity.HasOne<Usuario>().WithMany().HasForeignKey(a => a.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Docente>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Telefono).IsRequired().HasMaxLength(20);
            entity.Property(d => d.Categoria).IsRequired().HasMaxLength(100);
            entity.HasOne(d => d.Usuario)
                .WithOne()
                .HasForeignKey<Docente>(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(d => d.UsuarioId).IsUnique();
        });

        modelBuilder.Entity<Preceptor>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Telefono).IsRequired().HasMaxLength(20);
            entity.Property(p => p.Turno).IsRequired().HasMaxLength(50);
            entity.HasOne(p => p.Usuario)
                .WithOne()
                .HasForeignKey<Preceptor>(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(p => p.UsuarioId).IsUnique();
        });

        modelBuilder.Entity<Estudiante>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Anio).IsRequired();
            entity.Property(e => e.Condicion).IsRequired().HasConversion<string>();
            entity.Property(e => e.FechaDeIngreso).IsRequired();
            entity.HasOne(e => e.Usuario)
                .WithOne()
                .HasForeignKey<Estudiante>(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.UsuarioId).IsUnique();
        });

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
