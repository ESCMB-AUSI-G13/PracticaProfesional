using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Crea los 60 estudiantes Trayecto de la cohorte 2025 (1er año).
///
/// Distribución por comisión:
///   0 Egresados · 10 DesertoresY1 · 3 DesertoresY2 · 17 Regulares = 30
///
/// Sin egresados: la cohorte 2025 ingresó en marzo 2025 y la carrera dura 2 años.
/// El primer egreso posible sería en 2027, por lo que en 2026 nadie puede haber egresado.
///
/// Legajo: EST-H2025-C2{comision}-{n:D3}
/// DNI:    desde 41_009_001
/// Idempotente: si ya existen estudiantes Trayecto 2025, se omite.
/// </summary>
public static class NuevosEstudiantes2025TrayectoSeeder
{
    private static readonly string[] Nombres =
    [
        "Aída", "Bernardo", "Claudia", "Dante", "Eliana", "Fernando",
        "Gabriela", "Hilario", "Ingrid", "Jorge", "Kira", "Luciana",
        "Mateo", "Nidia", "Omar", "Pamela", "Quézia", "Roberto",
        "Sandra", "Tadeo", "Ursula", "Valentín", "Wendy", "Xavier",
        "Yadira", "Zamir", "Amalia", "Basilio", "Celina", "Damián"
    ];

    private static readonly string[] Apellidos =
    [
        "Agüero", "Barreto", "Castillo", "Dávila", "Espejo", "Fontana",
        "Grimaldi", "Herrero", "Islas", "Jaraba", "Kogan", "Londoño",
        "Mansilla", "Neyra", "Osorio", "Pereyra", "Quisbert", "Romero",
        "Santamaría", "Tejedor", "Urquijo", "Vargas", "Wippl", "Yucra",
        "Zelada", "Antezana", "Borjas", "Castellanos", "Díaz", "Encina"
    ];

    private const int Egresados    = 0;
    private const int DesertoresY1 = 10;
    private const int DesertoresY2 = 3;
    private const int Regulares    = 17; // 30 - 0 - 10 - 3 = 17

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        bool yaExisten = await db.Estudiantes
            .AnyAsync(e => e.FechaDeIngreso.Year == 2025 && e.CarreraId == 2, ct);

        if (yaExisten)
        {
            logger.LogInformation("NuevosEstudiantes2025TrayectoSeeder: Trayecto 2025 ya existe, omitido.");
            return;
        }

        var materiaIds = await db.Materias
            .Where(m => m.Anio == 1 && m.CarreraId == 2)
            .OrderBy(m => m.Id)
            .Select(m => m.Id)
            .ToListAsync(ct);

        var cursosPorComision = await db.Cursos
            .Where(c => c.Anio == 2025 && c.AnioLectivo == 1 && c.CarreraId == 2)
            .ToDictionaryAsync(c => c.Comision, c => c.Id, ct);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Alumno2026!");
        var fechaIngreso  = new DateTime(2025, 3, 1);

        int dniBase    = 41_009_001;
        int globalIdx  = 0;
        int creados    = 0;

        foreach (var comision in new[] { "A", "B" })
        {
            if (!cursosPorComision.TryGetValue(comision, out var cursoId))
            {
                logger.LogWarning("NuevosEstudiantes2025TrayectoSeeder: no encontró curso Trayecto Com{C} 2025.", comision);
                continue;
            }

            for (int i = 0; i < 30; i++, globalIdx++)
            {
                var (condicion, anio) = ClasificarAlumno(i);

                var nombre   = Nombres[globalIdx % Nombres.Length];
                var apellido = Apellidos[(globalIdx / Nombres.Length) % Apellidos.Length];
                var legajo   = $"EST-H2025-C2{comision}-{i + 1:D3}";
                var email    = $"alu.h2025.c2{comision.ToLower()}.{i + 1:D3}@institucion.edu.ar";

                var usuario = Usuario.Crear(
                    (dniBase++).ToString(), legajo, email,
                    nombre, apellido, passwordHash, Rol.Estudiante);
                db.Usuarios.Add(usuario);
                await db.SaveChangesAsync(ct);

                var estudiante = Estudiante.Crear(usuario.Id, anio, carreraId: 2, fechaIngreso);
                if (condicion == CondicionEstudiante.Desertor)
                    estudiante.Desertar();
                db.Estudiantes.Add(estudiante);
                await db.SaveChangesAsync(ct);

                foreach (var materiaId in materiaIds)
                    db.InscripcionesMateria.Add(InscripcionMateria.Crear(estudiante.Id, materiaId, cursoId));

                creados++;
            }

            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "NuevosEstudiantes2025TrayectoSeeder: C2{Com} — 30 alumnos creados ({D1}D-A1/{D2}D-A2/{R}R).",
                comision, DesertoresY1, DesertoresY2, Regulares);
        }

        var cursosIds = cursosPorComision.Values.ToList();
        await db.InscripcionesMateria
            .Where(i => cursosIds.Contains(i.CursoId))
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.FechaInscripcion, new DateTime(2025, 3, 1)), ct);

        logger.LogInformation("NuevosEstudiantes2025TrayectoSeeder: {N} estudiantes Trayecto 2025 creados.", creados);
    }

    // Y1 desertores primero, luego Y2 desertores, luego regulares
    private static (CondicionEstudiante Condicion, int Anio) ClasificarAlumno(int i)
    {
        if (i < DesertoresY1)                    return (CondicionEstudiante.Desertor, 1);
        if (i < DesertoresY1 + DesertoresY2)     return (CondicionEstudiante.Desertor, 2);
        return (CondicionEstudiante.Regular, 2);
    }
}
