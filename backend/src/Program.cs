using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PracticaProfesional.Application.Auditoria;
using PracticaProfesional.Application.Auth;
using PracticaProfesional.Application.LogsSeguridad;
using PracticaProfesional.Application.Docentes;
using PracticaProfesional.Application.Estudiantes;
using PracticaProfesional.Application.EstadoAcademico;
using PracticaProfesional.Application.Inscripciones;
using PracticaProfesional.Application.Reportes;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Preceptores;
using PracticaProfesional.Application.Usuarios;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Auditoria;
using PracticaProfesional.Infrastructure.Auth;
using PracticaProfesional.Infrastructure.Seguridad;
using PracticaProfesional.Infrastructure.Sesiones;
using PracticaProfesional.Infrastructure.Email;
using PracticaProfesional.Infrastructure.Persistence;
using PracticaProfesional.Infrastructure.Middleware;
using PracticaProfesional.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── Base de datos ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// ── Repositorios e interfaces ──────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
builder.Services.AddScoped<IAuditoriaLogRepository, AuditoriaLogRepository>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddSingleton<ISesionService, SesionService>();
builder.Services.AddScoped<ILogSeguridadRepository, LogSeguridadRepository>();
builder.Services.AddScoped<ILogSeguridadService, LogSeguridadService>();
builder.Services.AddScoped<ListarLogsLoginUseCase>();
builder.Services.AddScoped<IDocenteRepository, DocenteRepository>();
builder.Services.AddScoped<IPreceptorRepository, PreceptorRepository>();
builder.Services.AddScoped<IEstudianteRepository, EstudianteRepository>();
builder.Services.AddScoped<ICorrelativiadadRepository, CorrelativiadadRepository>();
builder.Services.AddScoped<IHistorialAcademicoRepository, HistorialAcademicoRepository>();
builder.Services.AddScoped<IInscripcionMateriaRepository, InscripcionMateriaRepository>();
builder.Services.AddScoped<IAsistenciaRepository, AsistenciaRepository>();
builder.Services.AddScoped<IMateriaRepository, MateriaRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();

// ── Use Cases ──────────────────────────────────────────────────────────────────
builder.Services.AddHttpClient();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<RegistroUseCase>();
builder.Services.AddScoped<SolicitarRestablecimientoUseCase>();
builder.Services.AddScoped<RestablecerPasswordUseCase>();
builder.Services.AddScoped<RegistrarCambioRolUseCase>();
builder.Services.AddScoped<ListarAuditoriaLogsUseCase>();
builder.Services.AddScoped<CrearUsuarioUseCase>();
builder.Services.AddScoped<ListarUsuariosUseCase>();
builder.Services.AddScoped<ModificarUsuarioUseCase>();
builder.Services.AddScoped<DesactivarUsuarioUseCase>();
builder.Services.AddScoped<ReactivarUsuarioUseCase>();

// Docentes
builder.Services.AddScoped<CrearDocenteUseCase>();
builder.Services.AddScoped<ListarDocentesUseCase>();
builder.Services.AddScoped<ModificarDocenteUseCase>();
builder.Services.AddScoped<DesactivarDocenteUseCase>();
builder.Services.AddScoped<ReactivarDocenteUseCase>();

// Preceptores
builder.Services.AddScoped<CrearPreceptorUseCase>();
builder.Services.AddScoped<ListarPreceptoresUseCase>();
builder.Services.AddScoped<ModificarPreceptorUseCase>();
builder.Services.AddScoped<DesactivarPreceptorUseCase>();
builder.Services.AddScoped<ReactivarPreceptorUseCase>();

// Inscripciones
builder.Services.AddScoped<InscribirseEnMateriaUseCase>();

// Estado Académico
builder.Services.AddScoped<ActualizarEstadoAcademicoUseCase>();

// Reportes Operativos (RR-08, RR-09)
builder.Services.AddScoped<ReporteInasistenciasUseCase>();
builder.Services.AddScoped<ControlIndividualPorLegajoUseCase>();

// Estudiantes
builder.Services.AddScoped<CrearEstudianteUseCase>();
builder.Services.AddScoped<ListarEstudiantesUseCase>();
builder.Services.AddScoped<ModificarEstudianteUseCase>();
builder.Services.AddScoped<DesactivarEstudianteUseCase>();
builder.Services.AddScoped<ReactivarEstudianteUseCase>();

// ── Autenticación JWT ──────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── CORS ───────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── Middleware global de excepciones ───────────────────────────────────────────
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddControllers();

var app = builder.Build();

// ── Migración automática + seed inicial ───────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        if (!db.Usuarios.Any())
        {
            var admin = Usuario.Crear(
                dni: "00000000",
                legajo: "ADMIN-001",
                email: "admin@institucion.edu.ar",
                nombre: "Administrador",
                apellido: "Sistema",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("Admin1234!"),
                rol: Rol.Direccion
            );
            db.Usuarios.Add(admin);
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al aplicar migraciones o seed. La aplicación continuará sin migración automática.");
    }
}

app.UseExceptionHandler();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
