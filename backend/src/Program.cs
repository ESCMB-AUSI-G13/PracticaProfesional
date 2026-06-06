using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PracticaProfesional.Application.Auditoria;
using PracticaProfesional.Application.Auth;
using PracticaProfesional.Application.Calificaciones;
using PracticaProfesional.Application.LogsSeguridad;
using PracticaProfesional.Application.Docentes;
using PracticaProfesional.Application.Estudiantes;
using PracticaProfesional.Application.EstadoAcademico;
using PracticaProfesional.Application.Inscripciones;
using PracticaProfesional.Application.Reportes;
using PracticaProfesional.Application.Materias;
using PracticaProfesional.Application.Carreras;
using PracticaProfesional.Application.Correlatividades;
using PracticaProfesional.Application.Calendario;
using PracticaProfesional.Application.Cursos;
using PracticaProfesional.Infrastructure.Seeding;
using PracticaProfesional.Application.Asistencias;
using PracticaProfesional.Application.EspaciosCurriculares;
using PracticaProfesional.Application.Examenes;
using PracticaProfesional.Application.Alertas;
using PracticaProfesional.Application.Notificaciones;
using PracticaProfesional.Application.Encuestas;
using PracticaProfesional.Application.Padron;
using PracticaProfesional.Infrastructure.BackgroundServices;
using PracticaProfesional.Infrastructure.Persistence.Repositories;
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
using PracticaProfesional.Domain.Exceptions;
using PracticaProfesional.Infrastructure.Pdf;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

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
builder.Services.AddScoped<ICalendarioAcademicoRepository, CalendarioAcademicoRepository>();
builder.Services.AddScoped<IHistorialAcademicoRepository, HistorialAcademicoRepository>();
builder.Services.AddScoped<IInscripcionMateriaRepository, InscripcionMateriaRepository>();
builder.Services.AddScoped<IAsistenciaRepository, AsistenciaRepository>();
builder.Services.AddScoped<IMateriaRepository, MateriaRepository>();
builder.Services.AddScoped<ICarreraRepository, CarreraRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPadronRepository, PadronRepository>();

// ── Use Cases ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<RegistroUseCase>();

// Padrón
builder.Services.AddScoped<CargarPadronUseCase>();
builder.Services.AddScoped<AgregarDniUseCase>();
builder.Services.AddScoped<ListarPadronUseCase>();
builder.Services.AddScoped<EliminarDniUseCase>();
builder.Services.AddScoped<SolicitarRestablecimientoUseCase>();
builder.Services.AddScoped<RestablecerPasswordUseCase>();
builder.Services.AddScoped<RegistrarCambioRolUseCase>();
builder.Services.AddScoped<ListarAuditoriaLogsUseCase>();
builder.Services.AddScoped<CrearUsuarioUseCase>();
builder.Services.AddScoped<ListarUsuariosUseCase>();
builder.Services.AddScoped<ModificarUsuarioUseCase>();
builder.Services.AddScoped<DesactivarUsuarioUseCase>();
builder.Services.AddScoped<ReactivarUsuarioUseCase>();
builder.Services.AddScoped<CambiarClaveUseCase>();

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
builder.Services.AddScoped<ListarInscripcionesUseCase>();
builder.Services.AddScoped<InscribirseEnMateriaUseCase>();
builder.Services.AddScoped<InscribirseEnMateriaAutogestUseCase>();
builder.Services.AddScoped<ListarMisInscripcionesEstudianteUseCase>();
builder.Services.AddScoped<ObtenerComprobanteInscripcionUseCase>();
builder.Services.AddScoped<DarDeBajaInscripcionMateriaUseCase>();
builder.Services.AddScoped<ObtenerComprobanteInscripcionExamenUseCase>();
builder.Services.AddScoped<IInscripcionExamenRepository, InscripcionExamenRepository>();

// Calificaciones
builder.Services.AddScoped<CargarNotaExamenUseCase>();
builder.Services.AddScoped<ListarInscripcionesExamenUseCase>();
builder.Services.AddScoped<RectificarNotaExamenUseCase>();
builder.Services.AddScoped<ObtenerHistorialNotasUseCase>();

// Estado Académico
builder.Services.AddScoped<ActualizarEstadoAcademicoUseCase>();

// Asistencias
builder.Services.AddScoped<ObtenerEspaciosPorDocenteUseCase>();
builder.Services.AddScoped<ObtenerAlumnosPorEspacioUseCase>();
builder.Services.AddScoped<RegistrarAsistenciasUseCase>();
builder.Services.AddScoped<ObtenerRegistroDelDiaUseCase>();
builder.Services.AddScoped<RectificarAsistenciaUseCase>();

// Reportes Operativos (RR-08, RR-09)
builder.Services.AddScoped<ReporteInasistenciasUseCase>();
builder.Services.AddScoped<ControlIndividualPorLegajoUseCase>();

// Carreras
builder.Services.AddScoped<ListarCarrerasUseCase>();

// Materias
builder.Services.AddScoped<CrearMateriaUseCase>();
builder.Services.AddScoped<ListarMateriasUseCase>();
builder.Services.AddScoped<ListarMateriasEstudianteUseCase>();
builder.Services.AddScoped<ModificarMateriaUseCase>();
builder.Services.AddScoped<EliminarMateriaUseCase>();

// Correlatividades
builder.Services.AddScoped<CrearCorrelativiadadUseCase>();
builder.Services.AddScoped<ListarCorrelativiadadesUseCase>();
builder.Services.AddScoped<EliminarCorrelativiadadUseCase>();

// Calendario Académico
builder.Services.AddScoped<ListarEventosCalendarioUseCase>();
builder.Services.AddScoped<CrearEventoCalendarioUseCase>();
builder.Services.AddScoped<ModificarEventoCalendarioUseCase>();
builder.Services.AddScoped<EliminarEventoCalendarioUseCase>();

// Cursos
builder.Services.AddScoped<ICursoRepository, CursoRepository>();
builder.Services.AddScoped<CrearCursoUseCase>();
builder.Services.AddScoped<ListarCursosUseCase>();
builder.Services.AddScoped<ModificarCursoUseCase>();
builder.Services.AddScoped<CerrarCursoUseCase>();
builder.Services.AddScoped<ReactivarCursoUseCase>();

// EspaciosCurriculares
builder.Services.AddScoped<IEspacioCurricularRepository, EspacioCurricularRepository>();
builder.Services.AddScoped<CrearEspacioCurricularUseCase>();
builder.Services.AddScoped<ListarEspaciosCurricularesUseCase>();
builder.Services.AddScoped<ListarEspaciosDocenteUseCase>();
builder.Services.AddScoped<EliminarEspacioCurricularUseCase>();

// Exámenes
builder.Services.AddScoped<IExamenRepository, ExamenRepository>();
builder.Services.AddScoped<CrearExamenUseCase>();
builder.Services.AddScoped<ListarExamenesUseCase>();
builder.Services.AddScoped<EliminarExamenUseCase>();
builder.Services.AddScoped<ListarFinalesDisponiblesUseCase>();
builder.Services.AddScoped<InscribirseEnExamenUseCase>();

// Reportes Rendimiento Consolidado (RR-05, RR-06, RR-07)
builder.Services.AddScoped<IRendimientoConsolidadoRepository, RendimientoConsolidadoRepository>();
builder.Services.AddScoped<ComparativoComisionesUseCase>();
builder.Services.AddScoped<EvolucionNotasUseCase>();
builder.Services.AddScoped<PromediosCatedraUseCase>();

// Reportes Cohorte — Riesgo Académico y Retención
builder.Services.AddScoped<RiesgoAcademicoUseCase>();
builder.Services.AddScoped<RetencionPorCohorteUseCase>();
builder.Services.AddScoped<TableroEjecutivoUseCase>();
builder.Services.AddScoped<RetencionAnualUseCase>();
builder.Services.AddScoped<DesercionPorAnioUseCase>();
builder.Services.AddScoped<EgresadosPorCarreraUseCase>();

// PDF
builder.Services.AddSingleton<PdfReporteService>();

// Encuestas (CU-36/CU-40)
builder.Services.AddScoped<IEncuestaRepository, EncuestaRepository>();
builder.Services.AddScoped<ListarEncuestasUseCase>();
builder.Services.AddScoped<CrearEncuestaUseCase>();
builder.Services.AddScoped<AgregarPreguntaUseCase>();
builder.Services.AddScoped<ActivarDesactivarEncuestaUseCase>();
builder.Services.AddScoped<ObtenerEncuestaPendienteUseCase>();
builder.Services.AddScoped<ResponderEncuestaUseCase>();
builder.Services.AddScoped<ResultadosEncuestasUseCase>();
builder.Services.AddScoped<ListarEncuestasDocenteUseCase>();
builder.Services.AddScoped<CrearEncuestaDocenteUseCase>();

// Alertas académicas y notificaciones internas
builder.Services.AddScoped<IAlertaRepository, AlertaRepository>();
builder.Services.AddScoped<INotificacionRepository, NotificacionRepository>();
builder.Services.AddScoped<ObtenerMisNotificacionesUseCase>();
builder.Services.AddScoped<MarcarNotificacionLeidaUseCase>();
builder.Services.AddScoped<MarcarTodasLeidasUseCase>();
builder.Services.AddScoped<DetectarRiesgoAcademicoUseCase>();
builder.Services.AddScoped<NotificarVencimientosUseCase>();
builder.Services.AddScoped<ListarAlertasUseCase>();
builder.Services.AddHostedService<AlertasBackgroundService>();

// Estudiantes
builder.Services.AddScoped<CrearEstudianteUseCase>();
builder.Services.AddScoped<ListarEstudiantesUseCase>();
builder.Services.AddScoped<ModificarEstudianteUseCase>();
builder.Services.AddScoped<DesactivarEstudianteUseCase>();
builder.Services.AddScoped<ReactivarEstudianteUseCase>();
builder.Services.AddScoped<EliminarEstudianteUseCase>();

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
        policy.WithOrigins(builder.Configuration["FrontendUrl"]!)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Sistema Académico API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresá el token JWT. Ejemplo: Bearer {token}"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Migración automática + seed inicial ───────────────────────────────────────
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Detecta y repara migraciones registradas en historia sin DDL ejecutado.
        // Causa: EnableRetryOnFailure interfiere con transacciones DDL de migrations.
        await RepararMigracionesInconsistentesAsync(db, logger);

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

        if (!db.Carreras.Any())
        {
            db.Carreras.Add(Carrera.Crear("Profesorado de Educación Secundaria en Economía", "Res. 0013"));
            db.Carreras.Add(Carrera.Crear("Trayecto Pedagógico para Graduados No Docentes", "Res. 104/22"));
            db.SaveChanges();
        }

        // ── SEEDERS DESHABILITADOS — no tocar datos sin consentimiento explícito ──
        // Para rehabilitar alguno, descomentar la línea correspondiente.

        // -- Estructurales (calendario, correlatividades, encuesta base) --
        // var calendarioRepo = scope.ServiceProvider.GetRequiredService<ICalendarioAcademicoRepository>();
        // await CalendarioSeeder.SeedAsync(calendarioRepo);
        // await CorrelativiadadesSeeder.SeedCarrera1Async(db, logger);
        // await CorrelativiadadesSeeder.SeedCarrera2Async(db, logger);
        // await CursosSeeder.SeedAsync(db, logger);
        // await EncuestaSeeder.SeedAsync(db);

        // -- Cohorte histórica 2021 --
        // await CohorteHistoricaSeeder.SeedAsync(db, logger);
        // await CohorteHistoricaSeeder.SeedInscripcionesCohorte2021Async(db, logger);
        // await Examenes2021Seeder.SeedAsync(db, logger);
        // await Notas2021Seeder.SeedAsync(db, logger);
        // await Encuestas2021Seeder.SeedAsync(db, logger);
        // await Asistencias2021Seeder.SeedAsync(db, logger);
        // await EspaciosCurriculares2021Seeder.SeedAsync(db, logger);
        // await HistorialAcademico2021Seeder.SeedAsync(db, logger);

        // -- Cohorte 2022 --
        // await NuevosEstudiantes2022Seeder.SeedAsync(db, logger);
        // await Anio2022ActividadesSeeder.SeedAsync(db, logger);

        // -- Cohorte 2023 --
        // await NuevosEstudiantes2023Seeder.SeedAsync(db, logger);
        // await Anio2023ActividadesSeeder.SeedAsync(db, logger);
        // await NuevosEstudiantes2023TrayectoSeeder.SeedAsync(db, logger);
        // await Anio2023ActividadesSeeder.SeedTrayecto2023Año1CompletarAsync(db, logger);

        // -- Cohorte 2024 --
        // await NuevosEstudiantes2024Seeder.SeedAsync(db, logger);
        // await Anio2024ActividadesSeeder.SeedAsync(db, logger);
        // await NuevosEstudiantes2024TrayectoSeeder.SeedAsync(db, logger);
        // await Anio2024ActividadesSeeder.SeedTrayecto2024Año1CompletarAsync(db, logger);

        // -- Cohorte 2025 --
        // await NuevosEstudiantes2025Seeder.SeedAsync(db, logger);
        // await Anio2025ActividadesSeeder.SeedAsync(db, logger);
        // await NuevosEstudiantes2025TrayectoSeeder.SeedAsync(db, logger);
        // await Anio2025ActividadesSeeder.SeedTrayecto2025Año1CompletarAsync(db, logger);

        // -- Cohorte 2026 --
        // await NuevosEstudiantes2026Seeder.SeedAsync(db, logger);
        // await Anio2026ActividadesSeeder.SeedAsync(db, logger);
        // await NuevosEstudiantes2026TrayectoSeeder.SeedAsync(db, logger);
        // await Anio2026ActividadesSeeder.SeedTrayecto2026Año1CompletarAsync(db, logger);

        // -- Correcciones / patches --
        await PatchDesercionSeeder.PatchAsync(db, logger);          // ← deserción real en cohortes 2025/2026 + año 4
        // await PatchEgresadosSeeder.PatchAsync(db, logger);       // ← HABILITAR para corregir tasas egresados
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al aplicar migraciones o seed. La aplicación continuará sin migración automática.");
    }
}

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema Académico API v1"));
}

// Manejo de excepciones DESPUÉS de CORS para que los headers no se pierdan
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        var (statusCode, title) = ex switch
        {
            BusinessException bex         => (bex.StatusCode, "Error de negocio"),
            UnauthorizedAccessException   => (StatusCodes.Status401Unauthorized, "No autorizado"),
            ArgumentException             => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            KeyNotFoundException          => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            InvalidOperationException     => (StatusCodes.Status409Conflict, "Conflicto de negocio"),
            _                             => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title  = title,
            Detail = ex.Message
        });
    }
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// ── Reparación de migraciones con DDL no aplicado ──────────────────────────────
// Si una migración está en __EFMigrationsHistory pero su DDL no se ejecutó
// (causado por EnableRetryOnFailure + transacciones DDL), la eliminamos para que
// db.Database.Migrate() la vuelva a aplicar correctamente.
static async Task RepararMigracionesInconsistentesAsync(
    PracticaProfesional.Infrastructure.Persistence.AppDbContext db,
    ILogger logger)
{
    // Mapa: (id de migración) → (SQL que verifica si el DDL se aplicó)
    var verificaciones = new Dictionary<string, string>
    {
        ["20260529000001_AddFechaDeEgresoEstudiante"]  =
            "SELECT COUNT(1) FROM sys.columns WHERE Name = N'FechaDeEgreso' AND Object_ID = Object_ID(N'Estudiantes')",
        ["20260529000002_EnsureFechaDeEgresoEstudiante"] =
            "SELECT COUNT(1) FROM sys.columns WHERE Name = N'FechaDeEgreso' AND Object_ID = Object_ID(N'Estudiantes')",
    };

    var conn = db.Database.GetDbConnection();
    if (conn.State != System.Data.ConnectionState.Open)
        await conn.OpenAsync();

    foreach (var (migrationId, sql) in verificaciones)
    {
        using var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = sql;
        int existe = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

        if (existe == 0)
        {
            // DDL nunca se ejecutó → borrar el registro fantasma para que Migrate() lo re-aplique
            using var delCmd = conn.CreateCommand();
            delCmd.CommandText =
                $"DELETE FROM [__EFMigrationsHistory] WHERE [MigrationId] = '{migrationId}'";
            await delCmd.ExecuteNonQueryAsync();
            logger.LogWarning(
                "DB-Fix: migración '{Id}' tenía DDL sin aplicar — registro eliminado de historia para re-aplicación.",
                migrationId);
        }
    }
}
