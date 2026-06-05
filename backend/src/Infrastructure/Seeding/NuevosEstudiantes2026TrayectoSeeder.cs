using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Crea los 60 estudiantes Trayecto de la cohorte 2026 (1er año, en curso).
///
/// Distribución por comisión:
///   0 Egresados · 3 DesertoresY1 · 0 DesertoresY2 · 27 Regulares = 30
///
/// Sin egresados: la cohorte 2026 ingresó en marzo 2026 (hace ~3 meses en junio 2026).
/// La carrera dura 2 años; el primer egreso posible sería en 2028.
/// Sin desertores Y2: el año 1 aún no terminó.
/// Solo 3 desertores Y1 por comisión (abandono muy temprano).
///
/// Legajo: EST-H2026-C2{comision}-{n:D3}
/// DNI:    desde 41_011_001
/// Idempotente: si ya existen estudiantes Trayecto 2026, se omite.
/// </summary>
public static class NuevosEstudiantes2026TrayectoSeeder
{
    private static readonly string[] Nombres =
    [
        "Alicia", "Boris", "Carmen", "David", "Eva", "Francisco",
        "Gloria", "Héctor", "Irene", "Juan", "Kiko", "Lila",
        "Marco", "Nora", "Óscar", "Pilar", "Quin", "Ramón",
        "Sonia", "Tino", "Úrsula", "Valerio", "Wanda", "Xandra",
        "Yolanda", "Zenón", "Aldo", "Bírgit", "Cósimo", "Dolores"
    ];

    private static readonly string[] Apellidos =
    [
        "Alfaro", "Berger", "Canales", "Demarchi", "Extebarría", "Figón",
        "Galaviz", "Holguín", "Irún", "Jiménez", "Kiefer", "Lugo",
        "Mancilla", "Navaja", "Orosco", "Pino", "Quero", "Reyes",
        "Sobrado", "Tufiño", "Utrera", "Vilca", "Witherspoon", "Yuste",
        "Zancudo", "Arenas", "Bolaño", "Cardona", "Degásperi", "Eguizábal"
    ];

    private const int Egresados    = 0;
    private const int DesertoresY1 = 3;
    private const int DesertoresY2 = 0;
    private const int Regulares    = 27; // 30 - 0 - 3 - 0 = 27

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        bool yaExisten = await db.Estudiantes
            .AnyAsync(e => e.FechaDeIngreso.Year == 2026 && e.CarreraId == 2, ct);

        if (yaExisten)
        {
            logger.LogInformation("NuevosEstudiantes2026TrayectoSeeder: Trayecto 2026 ya existe, omitido.");
            return;
        }

        var materiaIds = await db.Materias
            .Where(m => m.Anio == 1 && m.CarreraId == 2)
            .OrderBy(m => m.Id)
            .Select(m => m.Id)
            .ToListAsync(ct);

        var cursosPorComision = await db.Cursos
            .Where(c => c.Anio == 2026 && c.AnioLectivo == 1 && c.CarreraId == 2)
            .ToDictionaryAsync(c => c.Comision, c => c.Id, ct);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Alumno2026!");
        var fechaIngreso  = new DateTime(2026, 3, 1);

        int dniBase    = 41_011_001;
        int globalIdx  = 0;
        int creados    = 0;

        foreach (var comision in new[] { "A", "B" })
        {
            if (!cursosPorComision.TryGetValue(comision, out var cursoId))
            {
                logger.LogWarning("NuevosEstudiantes2026TrayectoSeeder: no encontró curso Trayecto Com{C} 2026.", comision);
                continue;
            }

            for (int i = 0; i < 30; i++, globalIdx++)
            {
                var (condicion, anio) = ClasificarAlumno(i);

                var nombre   = Nombres[globalIdx % Nombres.Length];
                var apellido = Apellidos[(globalIdx / Nombres.Length) % Apellidos.Length];
                var legajo   = $"EST-H2026-C2{comision}-{i + 1:D3}";
                var email    = $"alu.h2026.c2{comision.ToLower()}.{i + 1:D3}@institucion.edu.ar";

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
                "NuevosEstudiantes2026TrayectoSeeder: C2{Com} — 30 alumnos creados ({D1}D-A1/{R}R).",
                comision, DesertoresY1, Regulares);
        }

        var cursosIds = cursosPorComision.Values.ToList();
        await db.InscripcionesMateria
            .Where(i => cursosIds.Contains(i.CursoId))
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.FechaInscripcion, new DateTime(2026, 3, 1)), ct);

        logger.LogInformation("NuevosEstudiantes2026TrayectoSeeder: {N} estudiantes Trayecto 2026 creados.", creados);
    }

    // Primero Y1 desertores (abandono temprano año 1), luego regulares
    private static (CondicionEstudiante Condicion, int Anio) ClasificarAlumno(int i)
    {
        if (i < DesertoresY1) return (CondicionEstudiante.Desertor, 1);
        return (CondicionEstudiante.Regular, 1);
    }
}
