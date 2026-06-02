using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Crea los 60 estudiantes Trayecto de la cohorte 2023 (1er año).
///
/// Distribución por comisión (igual que CohorteHistoricaSeeder para Trayecto):
///   13 Egresados · 10 Desertores Año1 · 3 Desertores Año2 · 4 Regulares
///
/// Los inscribe en los cursos Año 1 Trayecto 2023.
/// Legajo: EST-H2023-C2{comision}-{n:D3}
/// DNI:    desde 41_005_001
/// Idempotente: si ya existen estudiantes Trayecto 2023, se omite.
/// </summary>
public static class NuevosEstudiantes2023TrayectoSeeder
{
    private static readonly string[] Nombres =
    [
        "Alejandro", "Bárbara", "César", "Diana", "Eduardo", "Fernanda",
        "Gonzalo", "Hilda", "Ignacio", "Jimena", "Kevin", "Laura",
        "Marcelo", "Noemí", "Oscar", "Patricia", "Quintín", "Rosa",
        "Sergio", "Tatiana", "Ulises", "Valeria", "Walter", "Ximena",
        "Yago", "Zoe", "Adrián", "Betina", "Claudio", "Dolores"
    ];

    private static readonly string[] Apellidos =
    [
        "Alvarado", "Barrios", "Carrasco", "Delgado", "Espinoza", "Figueras",
        "Gallardo", "Huerta", "Iriarte", "Juárez", "Kramer", "Lamas",
        "Mansilla", "Núñez", "Oviedo", "Ponce", "Quintero", "Rossi",
        "Sosa", "Tapia", "Ugarte", "Vallejos", "Wenceslao", "Yáñez",
        "Zamudio", "Abrego", "Balmaceda", "Carmona", "Dorado", "Escudero"
    ];

    // Distribución idéntica a CohorteHistoricaSeeder para Trayecto:
    // Egresados, Y1Desertores, Y2Desertores, Regulares
    private const int Egresados    = 13;
    private const int DesertoresY1 = 10;
    private const int DesertoresY2 = 3;
    private const int Regulares    = 4; // 30 - 13 - 10 - 3 = 4

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        bool yaExisten = await db.Estudiantes
            .AnyAsync(e => e.FechaDeIngreso.Year == 2023 && e.CarreraId == 2, ct);

        if (yaExisten)
        {
            logger.LogInformation("NuevosEstudiantes2023TrayectoSeeder: Trayecto 2023 ya existe, omitido.");
            return;
        }

        var materiaIds = await db.Materias
            .Where(m => m.Anio == 1 && m.CarreraId == 2)
            .OrderBy(m => m.Id)
            .Select(m => m.Id)
            .ToListAsync(ct);

        var cursosPorComision = await db.Cursos
            .Where(c => c.Anio == 2023 && c.AnioLectivo == 1 && c.CarreraId == 2)
            .ToDictionaryAsync(c => c.Comision, c => c.Id, ct);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Alumno2026!");
        var fechaIngreso  = new DateTime(2023, 3, 1);
        var rng           = new Random(88);
        int dniBase       = 41_005_001;
        int globalIdx     = 0;
        int creados       = 0;

        // FechaDeEgreso: cohorte 2023 + 2 años = egreso oct 2025 - mar 2026
        var egresoDesde = new DateTime(2025, 10, 1);
        var egresoHasta = new DateTime(2026, 3, 31);
        int egresoRango = (egresoHasta - egresoDesde).Days;

        foreach (var comision in new[] { "A", "B" })
        {
            if (!cursosPorComision.TryGetValue(comision, out var cursoId))
            {
                logger.LogWarning("NuevosEstudiantes2023TrayectoSeeder: no encontró curso Trayecto Com{C} 2023.", comision);
                continue;
            }

            var egresadosIds = new List<int>();

            for (int i = 0; i < 30; i++, globalIdx++)
            {
                var (condicion, anio) = ClasificarAlumno(i);

                var nombre   = Nombres[globalIdx % Nombres.Length];
                var apellido = Apellidos[(globalIdx / Nombres.Length) % Apellidos.Length];
                var legajo   = $"EST-H2023-C2{comision}-{i + 1:D3}";
                var email    = $"alu.h2023.c2{comision.ToLower()}.{i + 1:D3}@institucion.edu.ar";

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

                // Todos se inscriben en Año 1 (los desertores Y1 quedarán en Baja al procesar actividades)
                foreach (var materiaId in materiaIds)
                    db.InscripcionesMateria.Add(InscripcionMateria.Crear(estudiante.Id, materiaId, cursoId));

                creados++;
            }

            await db.SaveChangesAsync(ct);

            // FechaDeEgreso para egresados
            foreach (var id in egresadosIds)
            {
                var fecha = egresoDesde.AddDays(rng.Next(egresoRango));
                await db.Database.ExecuteSqlRawAsync(
                    "UPDATE Estudiantes SET FechaDeEgreso = {0} WHERE Id = {1}", fecha, id);
            }

            logger.LogInformation(
                "NuevosEstudiantes2023TrayectoSeeder: C2{Com} — 30 alumnos creados ({E}E/{D1}D-A1/{D2}D-A2/{R}R).",
                comision, Egresados, DesertoresY1, DesertoresY2, Regulares);
        }

        // Ajustar FechaInscripcion histórica
        var cursosIds = cursosPorComision.Values.ToList();
        await db.InscripcionesMateria
            .Where(i => cursosIds.Contains(i.CursoId))
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.FechaInscripcion, new DateTime(2023, 3, 1)), ct);

        logger.LogInformation("NuevosEstudiantes2023TrayectoSeeder: {N} estudiantes Trayecto 2023 creados.", creados);
    }

    // Egresados primero, luego Y1 desertores, Y2 desertores, regulares
    private static (CondicionEstudiante Condicion, int Anio) ClasificarAlumno(int i)
    {
        if (i < Egresados)                          return (CondicionEstudiante.Egresado, 2);
        if (i < Egresados + DesertoresY1)           return (CondicionEstudiante.Desertor, 1);
        if (i < Egresados + DesertoresY1 + DesertoresY2) return (CondicionEstudiante.Desertor, 2);
        return (CondicionEstudiante.Regular, 2);
    }
}
