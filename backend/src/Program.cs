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
using PracticaProfesional.Application.AsistenteIA;
using PracticaProfesional.Infrastructure.AsistenteIA;
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
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
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
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
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
builder.Services.AddScoped<CambiarActivacionUseCase>();
builder.Services.AddScoped<CambiarClaveUseCase>();

// Docentes
builder.Services.AddScoped<CrearDocenteUseCase>();
builder.Services.AddScoped<ListarDocentesUseCase>();
builder.Services.AddScoped<ModificarDocenteUseCase>();

// Preceptores
builder.Services.AddScoped<CrearPreceptorUseCase>();
builder.Services.AddScoped<ListarPreceptoresUseCase>();
builder.Services.AddScoped<ModificarPreceptorUseCase>();

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
builder.Services.AddScoped<PracticaProfesional.Application.MiHistorial.ObtenerMiHistorialUseCase>();

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
builder.Services.AddScoped<ListarCursosPorMateriaUseCase>();
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
builder.Services.AddScoped<InscribirseEnExamenAutogestUseCase>();
builder.Services.AddScoped<DarDeBajaInscripcionExamenUseCase>();

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

// Asistente IA (Dirección) — tool-calling sobre Reportes existentes (Gemini, tier gratuito)
builder.Services.AddScoped<IAsistenteIAService, AsistenteIAService>();
builder.Services.AddScoped<PreguntarAsistenteUseCase>();

// Estudiantes
builder.Services.AddScoped<CrearEstudianteUseCase>();
builder.Services.AddScoped<ListarEstudiantesUseCase>();
builder.Services.AddScoped<ModificarEstudianteUseCase>();

// ── Encuestas: salt de anonimización (CU-36/CU-40) ──────────────────────────────
// Sin este valor el token de disociación de identidad usaría un default público del
// código fuente, haciendo trivial recalcularlo y reidentificar quién respondió qué.
_ = builder.Configuration["Encuestas:Salt"]
    ?? throw new InvalidOperationException(
        "Falta configurar Encuestas:Salt (appsettings.Development.json en local, " +
        "Encuestas__Salt en Azure App Service). Generar un valor aleatorio largo, " +
        "no reutilizar ningún otro secreto.");

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

        // Sin esto, un JWT emitido antes de desactivar una cuenta seguía siendo válido hasta
        // su expiración (hasta 8h) — el JWT es stateless y por defecto no vuelve a chequear
        // Usuario.Activo. Se agrega una consulta liviana por Id (indexado) en cada request
        // autenticado para poder revocar el acceso de inmediato.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var usuarioIdClaim = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (usuarioIdClaim is null || !int.TryParse(usuarioIdClaim, out var usuarioId))
                {
                    context.Fail("Token inválido.");
                    return;
                }

                var usuarioRepository = context.HttpContext.RequestServices.GetRequiredService<IUsuarioRepository>();
                var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, context.HttpContext.RequestAborted);
                if (usuario is null || !usuario.Activo)
                {
                    context.Fail("La cuenta fue desactivada.");
                    return;
                }

                // Cierre remoto de sesión (ver CHECKLIST.md, Tier 7 #38): antes Dirección solo
                // podía "ver" quién estaba conectado (GET /activas), sin ninguna forma de cortar
                // el acceso de una sesión puntual sin desactivar la cuenta entera. Si hubo un
                // cierre forzado posterior a la emisión de este token, se rechaza — un login
                // nuevo emite un token válido de nuevo.
                var iatClaim = context.Principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Iat)?.Value;
                if (iatClaim is not null && long.TryParse(iatClaim, out var iatUnix))
                {
                    var emitidoEn = DateTimeOffset.FromUnixTimeSeconds(iatUnix).UtcDateTime;
                    var sesionService = context.HttpContext.RequestServices.GetRequiredService<ISesionService>();
                    if (sesionService.FueForzadoDespuesDe(usuarioId, emitidoEn))
                        context.Fail("La sesión fue cerrada remotamente. Volvé a iniciar sesión.");
                }
            }
        };
    });

builder.Services.AddAuthorization();

// ── Rate limiting (Asistente IA — control de costo del proveedor externo) ──────
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("asistente-ia", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonimo",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Sin esto, /api/auth/login no tenía ningún freno ante intentos de fuerza bruta —
    // verificado con 8 intentos fallidos consecutivos sin demora ni bloqueo. Se limita por IP
    // (antes de autenticar no hay otra identidad disponible); sin cola, así que el intento que
    // excede el límite se rechaza de inmediato en vez de esperar turno.
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonimo",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 8,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Demasiados intentos",
            Detail = "Se superó el límite de intentos. Esperá unos minutos y volvé a intentar."
        }, cancellationToken);
    };
});

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
            BusinessException bex when bex.StatusCode == 428 => (428, "Encuesta pendiente"),
            BusinessException bex         => (bex.StatusCode, "Error de negocio"),
            UnauthorizedAccessException   => (StatusCodes.Status401Unauthorized, "No autorizado"),
            ArgumentException             => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            KeyNotFoundException          => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            Microsoft.EntityFrameworkCore.DbUpdateException => (StatusCodes.Status409Conflict, "Conflicto de datos"),
            InvalidOperationException     => (StatusCodes.Status409Conflict, "Conflicto de negocio"),
            _                             => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        // Para errores no controlados (500) no se expone ex.Message: puede contener
        // detalles internos (cadenas de conexión, nombres de tabla, etc.). DbUpdateException
        // tampoco expone ex.Message (es boilerplate de EF, no describe el conflicto real y
        // su InnerException sí puede traer texto de SQL Server) — se usa un mensaje genérico
        // fijo. Para el resto, el mensaje ya está pensado para mostrarse al usuario.
        var detail = ex switch
        {
            Microsoft.EntityFrameworkCore.DbUpdateException =>
                "El cambio no se pudo guardar por un conflicto con los datos existentes (por ejemplo, un valor duplicado o una referencia inválida).",
            _ when statusCode == StatusCodes.Status500InternalServerError =>
                "Ocurrió un error interno. Intentá nuevamente más tarde.",
            _ => ex.Message
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title  = title,
            Detail = detail
        });
    }
});

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();

// ── Reparación de columnas con DDL no aplicado ──────────────────────────────────
// Antes esta función borraba el registro de la migración en __EFMigrationsHistory
// para que db.Database.Migrate() la volviera a aplicar. Verificado que NO es
// confiable: Migrate() puede reportar "already up to date" y no reaplicar el DDL
// igual (causa exacta no determinada — posiblemente interacción entre el DELETE
// manual sobre la conexión ya abierta y el chequeo de pendientes de Migrate()).
// En su lugar, esta versión aplica el DDL faltante de forma directa e idempotente,
// sin tocar __EFMigrationsHistory. Se salta si la tabla base todavía no existe
// (Migrate() la va a crear completa, columna incluida) — esto también evita el
// crash original en una base de datos totalmente vacía.
static async Task RepararMigracionesInconsistentesAsync(
    PracticaProfesional.Infrastructure.Persistence.AppDbContext db,
    ILogger logger)
{
    var fixes = new (string Tabla, string Columna, string Ddl)[]
    {
        ("Estudiantes", "FechaDeEgreso", "ALTER TABLE [Estudiantes] ADD [FechaDeEgreso] datetime2 NULL;"),
    };

    var conn = db.Database.GetDbConnection();
    if (conn.State != System.Data.ConnectionState.Open)
        await conn.OpenAsync();

    foreach (var (tabla, columna, ddl) in fixes)
    {
        using var tablaCmd = conn.CreateCommand();
        tablaCmd.CommandText = "SELECT COUNT(1) FROM sys.tables WHERE Name = @tabla";
        var pTabla = tablaCmd.CreateParameter();
        pTabla.ParameterName = "@tabla";
        pTabla.Value = tabla;
        tablaCmd.Parameters.Add(pTabla);
        bool tablaExiste = Convert.ToInt32(await tablaCmd.ExecuteScalarAsync()) > 0;
        if (!tablaExiste) continue;

        using var colCmd = conn.CreateCommand();
        colCmd.CommandText =
            "SELECT COUNT(1) FROM sys.columns WHERE Name = @columna AND Object_ID = Object_ID(@tabla)";
        var pColumna = colCmd.CreateParameter();
        pColumna.ParameterName = "@columna";
        pColumna.Value = columna;
        colCmd.Parameters.Add(pColumna);
        var pTabla2 = colCmd.CreateParameter();
        pTabla2.ParameterName = "@tabla";
        pTabla2.Value = tabla;
        colCmd.Parameters.Add(pTabla2);
        bool columnaExiste = Convert.ToInt32(await colCmd.ExecuteScalarAsync()) > 0;
        if (columnaExiste) continue;

        using var ddlCmd = conn.CreateCommand();
        ddlCmd.CommandText = ddl;
        await ddlCmd.ExecuteNonQueryAsync();
        logger.LogWarning(
            "DB-Fix: columna '{Columna}' de '{Tabla}' faltaba — agregada directamente (DDL idempotente, sin tocar __EFMigrationsHistory).",
            columna, tabla);
    }
}

// Necesario para WebApplicationFactory en tests de integración
public partial class Program { }
