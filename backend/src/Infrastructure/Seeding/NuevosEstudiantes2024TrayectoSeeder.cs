using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Crea los 60 estudiantes Trayecto de la cohorte 2024 (1er año).
///
/// CohorteHistoricaSeeder los tiene en su Grupos pero su guard siempre
/// salta cuando ya existen datos de 2021 — este seeder los suple.
///
/// Distribución por comisión (igual que CohorteHistoricaSeeder para Trayecto):
///   13 Egresados · 10 DesertoresY1 · 3 DesertoresY2 · 4 Regulares = 30
///
/// Legajo: EST-H2024-C2{comision}-{n:D3}
/// DNI:    desde 41_007_001
/// FechaDeEgreso: entre 2026-10-01 y 2027-03-31 (aún en el futuro en 2026).
/// Idempotente: si ya existen estudiantes Trayecto 2024, se omite.
/// </summary>
public static class NuevosEstudiantes2024TrayectoSeeder
{
    private static readonly string[] Nombres =
    [
        "Alexia", "Bruno", "Candela", "Darío", "Elena", "Federico",
        "Gisela", "Hugo", "Iris", "Javier", "Kira", "Lorenzo",
        "Micaela", "Naomi", "Osvaldo", "Priscila", "Quirino", "Rebeca",
        "Sebastián", "Talia", "Uriel", "Viviana", "Winston", "Ximena",
        "Yolanda", "Zenón", "Agostina", "Bautista", "Camila", "Diego"
    ];

    private static readonly string[] Apellidos =
    [
        "Acosta", "Benitez", "Cabrera", "Delmonte", "Escobar", "Figueroa",
        "Godoy", "Herrera", "Insúa", "Juárez", "Kirschbaum", "Luján",
        "Medina", "Neira", "Olmedo", "Padilla", "Quispe", "Ramos",
        "Soria", "Torres", "Urquiza", "Vega", "Windauer", "Yánez",
        "Zúñiga", "Arredondo", "Bravo", "Cisneros", "Dávila", "Espejo"
    ];

    private const int Egresados    = 13;
    private const int DesertoresY1 = 10;
    private const int DesertoresY2 = 3;
    private const int Regulares    = 4; // 30 - 13 - 10 - 3 = 4

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        bool yaExisten = await db.Estudiantes
            .AnyAsync(e => e.FechaDeIngreso.Year == 2024 && e.CarreraId == 2, ct);

        if (yaExisten)
        {
            logger.LogInformation("NuevosEstudiantes2024TrayectoSeeder: Trayecto 2024 ya existe, omitido.");
            return;
        }

        var materiaIds = await db.Materias
            .Where(m => m.Anio == 1 && m.CarreraId == 2)
            .OrderBy(m => m.Id)
            .Select(m => m.Id)
            .ToListAsync(ct);

        var cursosPorComision = await db.Cursos
            .Where(c => c.Anio == 2024 && c.AnioLectivo == 1 && c.CarreraId == 2)
            .ToDictionaryAsync(c => c.Comision, c => c.Id, ct);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Alumno2026!");
        var fechaIngreso  = new DateTime(2024, 3, 1);
        var rng           = new Random(99);
        // Egreso: Trayecto 2024 → cohorte+duración = 2024+2 = 2026 → oct 2026 – mar 2027
        var egresoDesde   = new DateTime(2026, 10, 1);
        var egresoHasta   = new DateTime(2027, 3, 31);
        int egresoRango   = (egresoHasta - egresoDesde).Days;

        int dniBase    = 41_007_001;
        int globalIdx  = 0;
        int creados    = 0;

        foreach (var comision in new[] { "A", "B" })
        {
            if (!cursosPorComision.TryGetValue(comision, out var cursoId))
            {
                logger.LogWarning("NuevosEstudiantes2024TrayectoSeeder: no encontró curso Trayecto Com{C} 2024.", comision);
                continue;
            }

            var egresadosIds = new List<int>();

            for (int i = 0; i < 30; i++, globalIdx++)
            {
                var (condicion, anio) = ClasificarAlumno(i);

                var nombre   = Nombres[globalIdx % Nombres.Length];
                var apellido = Apellidos[(globalIdx / Nombres.Length) % Apellidos.Length];
                var legajo   = $"EST-H2024-C2{comision}-{i + 1:D3}";
                var email    = $"alu.h2024.c2{comision.ToLower()}.{i + 1:D3}@institucion.edu.ar";

                var usuario = Usuario.Crear(
                    (dniBase++).ToString(), legajo, email,
                    nombre, apellido, passwordHash, Rol.Estudiante);
                db.Usuarios.Add(usuario);
                await db.SaveChangesAsync(ct);

                var estudiante = Estudiante.Crear(usuario.Id, anio, carreraId: 2, fechaIngreso);
                switch (condicion)
                {
                    case CondicionEstudiante.Egresado: estudiante.Egresar();  break;
                    case CondicionEstudiante.Desertor: estudiante.Desertar(); break;
                }
                db.Estudiantes.Add(estudiante);
                await db.SaveChangesAsync(ct);

                if (condicion == CondicionEstudiante.Egresado)
                    egresadosIds.Add(estudiante.Id);

                foreach (var materiaId in materiaIds)
                    db.InscripcionesMateria.Add(InscripcionMateria.Crear(estudiante.Id, materiaId, cursoId));

                creados++;
            }

            await db.SaveChangesAsync(ct);

            // FechaDeEgreso futura para egresados (oct 2026 – mar 2027)
            foreach (var id in egresadosIds)
            {
                var fecha = egresoDesde.AddDays(rng.Next(egresoRango));
                await db.Database.ExecuteSqlRawAsync(
                    "UPDATE Estudiantes SET FechaDeEgreso = {0} WHERE Id = {1}", fecha, id);
            }

            logger.LogInformation(
                "NuevosEstudiantes2024TrayectoSeeder: C2{Com} — 30 alumnos creados ({E}E/{D1}D-A1/{D2}D-A2/{R}R).",
                comision, Egresados, DesertoresY1, DesertoresY2, Regulares);
        }

        var cursosIds = cursosPorComision.Values.ToList();
        await db.InscripcionesMateria
            .Where(i => cursosIds.Contains(i.CursoId))
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.FechaInscripcion, new DateTime(2024, 3, 1)), ct);

        logger.LogInformation("NuevosEstudiantes2024TrayectoSeeder: {N} estudiantes Trayecto 2024 creados.", creados);
    }

    // Egresados primero, luego Y1 desertores, Y2 desertores, regulares
    private static (CondicionEstudiante Condicion, int Anio) ClasificarAlumno(int i)
    {
        if (i < Egresados)                                         return (CondicionEstudiante.Egresado, 2);
        if (i < Egresados + DesertoresY1)                         return (CondicionEstudiante.Desertor, 1);
        if (i < Egresados + DesertoresY1 + DesertoresY2)          return (CondicionEstudiante.Desertor, 2);
        return (CondicionEstudiante.Regular, 2);
    }
}
