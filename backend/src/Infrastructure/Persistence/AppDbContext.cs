using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Usuarios y roles
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Docente> Docentes => Set<Docente>();
    public DbSet<Preceptor> Preceptores => Set<Preceptor>();
    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();

    // Plan académico
    public DbSet<Materia> Materias => Set<Materia>();
    public DbSet<Correlatividad> Correlatividades => Set<Correlatividad>();
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<EspacioCurricular> EspaciosCurriculares => Set<EspacioCurricular>();

    // Inscripciones
    public DbSet<InscripcionMateria> InscripcionesMateria => Set<InscripcionMateria>();
    public DbSet<InscripcionExamen> InscripcionesExamen => Set<InscripcionExamen>();

    // Exámenes y asistencia
    public DbSet<Examen> Examenes => Set<Examen>();
    public DbSet<Asistencia> Asistencias => Set<Asistencia>();

    // Historial y seguimiento
    public DbSet<HistorialAcademico> HistorialAcademico => Set<HistorialAcademico>();
    public DbSet<Alerta> Alertas => Set<Alerta>();
    public DbSet<CalendarioAcademico> CalendarioAcademico => Set<CalendarioAcademico>();

    // Encuestas
    public DbSet<Encuesta> Encuestas => Set<Encuesta>();
    public DbSet<RespuestaEncuesta> RespuestasEncuesta => Set<RespuestaEncuesta>();

    // Auditoría
    public DbSet<AuditoriaCambio> AuditoriaCambios => Set<AuditoriaCambio>();
    public DbSet<AuditoriaCambioRol> AuditoriaCambiosRol => Set<AuditoriaCambioRol>();
    public DbSet<AuditoriaLog> AuditoriaLogs => Set<AuditoriaLog>();
    public DbSet<LogSeguridad> LogsSeguridad => Set<LogSeguridad>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ──────────────────────────────────────────────
        // LogSeguridad
        // ──────────────────────────────────────────────
        modelBuilder.Entity<LogSeguridad>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Email).IsRequired().HasMaxLength(150);
            entity.Property(l => l.Exitoso).IsRequired();
            entity.Property(l => l.MotivoFallo).IsRequired(false).HasMaxLength(200);
            entity.Property(l => l.IpOrigen).IsRequired().HasMaxLength(45);
            entity.Property(l => l.UserAgent).IsRequired().HasMaxLength(500);
            entity.Property(l => l.Timestamp).IsRequired();
            entity.HasIndex(l => l.Email);
            entity.HasIndex(l => l.Exitoso);
            entity.HasIndex(l => l.Timestamp);
        });

        // ──────────────────────────────────────────────
        // AuditoriaLog
        // ──────────────────────────────────────────────
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

        // ──────────────────────────────────────────────
        // AuditoriaCambioRol
        // ──────────────────────────────────────────────
        modelBuilder.Entity<AuditoriaCambioRol>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.RolOriginal).IsRequired().HasMaxLength(50);
            entity.Property(a => a.RolVista).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Accion).IsRequired().HasMaxLength(20);
            entity.Property(a => a.Timestamp).IsRequired();
            entity.HasOne<Usuario>().WithMany().HasForeignKey(a => a.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        // ──────────────────────────────────────────────
        // Usuario
        // ──────────────────────────────────────────────
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.DNI).IsRequired().HasMaxLength(10);
            entity.HasIndex(u => u.DNI).IsUnique();
            entity.Property(u => u.Legajo).IsRequired().HasMaxLength(20);
            entity.HasIndex(u => u.Legajo).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Apellido).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Rol).IsRequired().HasConversion<string>();
            entity.Property(u => u.Activo).IsRequired().HasDefaultValue(true);
            entity.Property(u => u.FechaCreacion).IsRequired();
        });

        // ──────────────────────────────────────────────
        // Docente
        // ──────────────────────────────────────────────
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

        // ──────────────────────────────────────────────
        // Preceptor
        // ──────────────────────────────────────────────
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

        // ──────────────────────────────────────────────
        // Estudiante
        // ──────────────────────────────────────────────
        modelBuilder.Entity<Estudiante>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Anio).IsRequired();
            entity.Property(e => e.Plan).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Condicion).IsRequired().HasConversion<string>();
            entity.Property(e => e.FechaDeIngreso).IsRequired();
            entity.HasOne(e => e.Usuario)
                .WithOne()
                .HasForeignKey<Estudiante>(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.UsuarioId).IsUnique();
        });

        // ──────────────────────────────────────────────
        // Materia
        // ──────────────────────────────────────────────
        modelBuilder.Entity<Materia>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Codigo).IsRequired().HasMaxLength(20);
            entity.HasIndex(m => m.Codigo).IsUnique();
            entity.Property(m => m.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Plan).IsRequired().HasMaxLength(20);
        });

        // ──────────────────────────────────────────────
        // Correlatividad
        // ──────────────────────────────────────────────
        modelBuilder.Entity<Correlatividad>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.TipoRequerimiento).IsRequired().HasMaxLength(50);
            entity.Property(c => c.CondicionAcademica).IsRequired().HasConversion<string>();
            entity.HasOne(c => c.MateriaDestino)
                .WithMany(m => m.CorrelativasDependientes)
                .HasForeignKey(c => c.MateriaDestinoId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(c => c.MateriaRequisito)
                .WithMany(m => m.CorrelativasRequisito)
                .HasForeignKey(c => c.MateriaRequisitoId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(c => new { c.MateriaDestinoId, c.MateriaRequisitoId, c.TipoRequerimiento }).IsUnique();
        });

        // ──────────────────────────────────────────────
        // Curso
        // ──────────────────────────────────────────────
        modelBuilder.Entity<Curso>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Anio).IsRequired();
            entity.Property(c => c.AnioLectivo).IsRequired();
            entity.Property(c => c.Comision).IsRequired().HasMaxLength(20);
            entity.Property(c => c.Cupo).IsRequired();
            entity.Property(c => c.Estado).IsRequired().HasConversion<string>();
            entity.HasOne(c => c.Preceptor)
                .WithMany()
                .HasForeignKey(c => c.PreceptorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(c => new { c.Anio, c.AnioLectivo, c.Comision }).IsUnique();
        });

        // ──────────────────────────────────────────────
        // EspacioCurricular
        // ──────────────────────────────────────────────
        modelBuilder.Entity<EspacioCurricular>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Materia).WithMany().HasForeignKey(e => e.MateriaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Docente).WithMany().HasForeignKey(e => e.DocenteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Curso).WithMany().HasForeignKey(e => e.CursoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.MateriaId, e.DocenteId, e.CursoId }).IsUnique();
        });

        // ──────────────────────────────────────────────
        // InscripcionMateria
        // ──────────────────────────────────────────────
        modelBuilder.Entity<InscripcionMateria>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Estado).IsRequired().HasConversion<string>();
            entity.Property(i => i.FechaInscripcion).IsRequired();
            entity.HasOne(i => i.Estudiante).WithMany().HasForeignKey(i => i.EstudianteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(i => i.Materia).WithMany().HasForeignKey(i => i.MateriaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(i => i.Curso).WithMany().HasForeignKey(i => i.CursoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(i => new { i.EstudianteId, i.MateriaId, i.CursoId }).IsUnique();
        });

        // ──────────────────────────────────────────────
        // Examen
        // ──────────────────────────────────────────────
        modelBuilder.Entity<Examen>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FechaExamen).IsRequired();
            entity.Property(e => e.Horario).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Cupo).IsRequired();
            entity.Property(e => e.TipoExamen).IsRequired().HasConversion<string>();
            entity.HasOne(e => e.Materia).WithMany().HasForeignKey(e => e.MateriaId).OnDelete(DeleteBehavior.Restrict);
        });

        // ──────────────────────────────────────────────
        // InscripcionExamen
        // ──────────────────────────────────────────────
        modelBuilder.Entity<InscripcionExamen>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Estado).IsRequired().HasConversion<string>();
            entity.Property(i => i.NotaValor).IsRequired(false).HasColumnType("decimal(4,2)");
            entity.Property(i => i.FechaInscripcion).IsRequired();
            entity.HasOne(i => i.Estudiante).WithMany().HasForeignKey(i => i.EstudianteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(i => i.Examen).WithMany().HasForeignKey(i => i.ExamenId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(i => new { i.EstudianteId, i.ExamenId }).IsUnique();
        });

        // ──────────────────────────────────────────────
        // Asistencia
        // ──────────────────────────────────────────────
        modelBuilder.Entity<Asistencia>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Fecha).IsRequired();
            entity.Property(a => a.Estado).IsRequired().HasConversion<string>();
            entity.Property(a => a.Motivo).IsRequired(false).HasMaxLength(300);
            entity.HasOne(a => a.Estudiante).WithMany().HasForeignKey(a => a.EstudianteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Materia).WithMany().HasForeignKey(a => a.MateriaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Curso).WithMany().HasForeignKey(a => a.CursoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(a => new { a.EstudianteId, a.MateriaId, a.CursoId, a.Fecha }).IsUnique();
        });

        // ──────────────────────────────────────────────
        // HistorialAcademico
        // ──────────────────────────────────────────────
        modelBuilder.Entity<HistorialAcademico>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Anio).IsRequired();
            entity.Property(h => h.Comision).IsRequired().HasMaxLength(20);
            entity.Property(h => h.EstadoFinal).IsRequired().HasMaxLength(50);
            entity.Property(h => h.NotaFinal).IsRequired(false).HasColumnType("decimal(4,2)");
            entity.Property(h => h.Condicion).IsRequired().HasConversion<string>();
            entity.HasOne(h => h.Estudiante).WithMany().HasForeignKey(h => h.EstudianteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(h => h.Materia).WithMany().HasForeignKey(h => h.MateriaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(h => h.Curso).WithMany().HasForeignKey(h => h.CursoId).OnDelete(DeleteBehavior.Restrict);
        });

        // ──────────────────────────────────────────────
        // Alerta
        // ──────────────────────────────────────────────
        modelBuilder.Entity<Alerta>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Anio).IsRequired();
            entity.Property(a => a.Comision).IsRequired().HasMaxLength(20);
            entity.Property(a => a.EstadoFinal).IsRequired().HasMaxLength(50);
            entity.Property(a => a.NotaFinal).IsRequired(false).HasColumnType("decimal(4,2)");
            entity.Property(a => a.Condicion).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Enviada).IsRequired();
            entity.Property(a => a.FechaCreacion).IsRequired();
            entity.HasOne(a => a.InscripcionExamen).WithMany().HasForeignKey(a => a.InscripcionExamenId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.InscripcionMateria).WithMany().HasForeignKey(a => a.InscripcionMateriaId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Examen).WithMany().HasForeignKey(a => a.ExamenId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        // ──────────────────────────────────────────────
        // CalendarioAcademico
        // ──────────────────────────────────────────────
        modelBuilder.Entity<CalendarioAcademico>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.NombreEvento).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Comision).HasMaxLength(20);
            entity.Property(c => c.FechaInicio).IsRequired();
            entity.Property(c => c.FechaFin).IsRequired();
            entity.Property(c => c.TipoEvento).IsRequired().HasConversion<string>();
            entity.HasOne(c => c.Materia).WithMany().HasForeignKey(c => c.MateriaId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(c => c.Curso).WithMany().HasForeignKey(c => c.CursoId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        // ──────────────────────────────────────────────
        // Encuesta
        // ──────────────────────────────────────────────
        modelBuilder.Entity<Encuesta>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Preguntas).IsRequired();
            entity.Property(e => e.Activa).IsRequired();
            entity.Property(e => e.FechaCreacion).IsRequired();
            entity.HasOne(e => e.Materia).WithMany().HasForeignKey(e => e.MateriaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Docente).WithMany().HasForeignKey(e => e.DocenteId).OnDelete(DeleteBehavior.Restrict);
        });

        // ──────────────────────────────────────────────
        // RespuestaEncuesta
        // ──────────────────────────────────────────────
        modelBuilder.Entity<RespuestaEncuesta>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Preguntas).IsRequired();
            entity.Property(r => r.Respuestas).IsRequired();
            entity.Property(r => r.Fecha).IsRequired();
            entity.HasOne(r => r.Encuesta)
                .WithMany(e => e.Respuestas)
                .HasForeignKey(r => r.EncuestaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ──────────────────────────────────────────────
        // AuditoriaCambio
        // ──────────────────────────────────────────────
        modelBuilder.Entity<AuditoriaCambio>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.TablaAfectada).IsRequired().HasMaxLength(100);
            entity.Property(a => a.RegistroAfectado).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Accion).IsRequired().HasMaxLength(50);
            entity.Property(a => a.FechaCambio).IsRequired();
            entity.Property(a => a.ValorAnterior).IsRequired(false);
            entity.Property(a => a.ValorNuevo).IsRequired(false);
            entity.HasOne(a => a.Usuario).WithMany().HasForeignKey(a => a.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Examen).WithMany().HasForeignKey(a => a.ExamenId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Calendario).WithMany().HasForeignKey(a => a.CalendarioId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.InscripcionExamen).WithMany().HasForeignKey(a => a.InscripcionExamenId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.InscripcionMateria).WithMany().HasForeignKey(a => a.InscripcionMateriaId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Encuesta).WithMany().HasForeignKey(a => a.EncuestaId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(a => a.TablaAfectada);
            entity.HasIndex(a => a.FechaCambio);
        });
    }
}
