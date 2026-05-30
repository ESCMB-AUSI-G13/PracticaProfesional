using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Crea los 120 estudiantes de la nueva cohorte 2022 (1er año).
///
/// 4 grupos de 30 alumnos:
///   Profesorado ComA, Profesorado ComB (CarreraId=1, AnioLectivo=1, Anio=2022)
///   Trayecto   ComA, Trayecto   ComB  (CarreraId=2, AnioLectivo=1, Anio=2022)
///
/// Todos inician con Condicion=Regular.
/// Las condiciones finales (Promo/Regular/Libre/Desertor) se asignan
/// al finalizar el año lectivo 2022 con los resultados reales.
///
/// InscripcionesMateria: creadas como Activa para todas las materias de 1er año.
/// Legajo: EST-H2022-C{carreraId}{comision}-{n:D3}
/// DNI:    desde 41_002_001
/// Idempotente: si ya existen estudiantes de la cohorte 2022, se omite.
/// </summary>
public static class NuevosEstudiantes2022Seeder
{
    private static readonly string[] Nombres =
    [
        "Agustín", "Belén", "Carlos", "Daniela", "Emilio", "Florencia",
        "Gustavo", "Hernán", "Irina", "Julián", "Karen", "Leonardo",
        "Magdalena", "Nicolás", "Olivia", "Pablo", "Quintina", "Rodrigo",
        "Sabrina", "Tomás", "Ursula", "Vanesa", "Walter", "Ximena",
        "Yésica", "Zacarías", "Andrea", "Braian", "Celeste", "Diego"
    ];

    private static readonly string[] Apellidos =
    [
        "Aguirre", "Bustos", "Contreras", "Dávila", "Echeverría", "Ferreira",
        "Godoy", "Heredia", "Insua", "Juárez", "Kramer", "Leguizamón",
        "Montoya", "Navarro", "Ocampo", "Palacio", "Quiroga", "Roldán",
        "Soldano", "Trujillo", "Ugarte", "Villanueva", "Wenceslao", "Ybarra",
        "Zárate", "Antunez", "Britos", "Cejas", "Doria", "Estrada"
    ];

    private record Grupo(int CarreraId, string Comision);

    private static readonly Grupo[] Grupos =
    [
        new(1, "A"), new(1, "B"),
        new(2, "A"), new(2, "B"),
    ];

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // Guard
        bool yaExisten = await db.Estudiantes
            .AnyAsync(e => e.FechaDeIngreso.Year == 2022, ct);

        if (yaExisten)
        {
            logger.LogInformation("NuevosEstudiantes2022Seeder: cohorte 2022 ya existe, omitido.");
            return;
        }

        // Materias de 1er año por carrera
        var materiasPorCarrera = await db.Materias
            .Where(m => m.Anio == 1 && (m.CarreraId == 1 || m.CarreraId == 2))
            .GroupBy(m => m.CarreraId)
            .ToDictionaryAsync(g => g.Key, g => g.OrderBy(m => m.Id).Select(m => m.Id).ToList(), ct);

        // Cursos de 1er año de 2022 indexados por (CarreraId, Comision)
        var cursosPorCarreraComision = await db.Cursos
            .Where(c => c.Anio == 2022 && c.AnioLectivo == 1)
            .ToDictionaryAsync(c => (c.CarreraId, c.Comision), c => c.Id, ct);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Alumno2026!");
        var fechaIngreso = new DateTime(2022, 3, 1);
        int dniBase      = 41_002_001;
        int globalIdx    = 0;
        int creados      = 0;

        foreach (var grupo in Grupos)
        {
            if (!materiasPorCarrera.TryGetValue(grupo.CarreraId, out var materiaIds))
                continue;

            if (!cursosPorCarreraComision.TryGetValue((grupo.CarreraId, grupo.Comision), out var cursoId))
            {
                logger.LogWarning("NuevosEstudiantes2022Seeder: no encontró curso para C{C}{Com} 2022.",
                    grupo.CarreraId, grupo.Comision);
                continue;
            }

            for (int i = 0; i < 30; i++, globalIdx++)
            {
                var nombre   = Nombres[globalIdx % Nombres.Length];
                var apellido = Apellidos[(globalIdx / Nombres.Length) % Apellidos.Length];
                var legajo   = $"EST-H2022-C{grupo.CarreraId}{grupo.Comision}-{i + 1:D3}";
                var email    = $"alu.h2022.c{grupo.CarreraId}{grupo.Comision.ToLower()}.{i + 1:D3}@institucion.edu.ar";

                var usuario = Usuario.Crear(
                    (dniBase++).ToString(), legajo, email,
                    nombre, apellido, passwordHash, Rol.Estudiante);
                db.Usuarios.Add(usuario);
                await db.SaveChangesAsync(ct);

                // Año 1 del plan de estudios, condición inicial Regular
                var estudiante = Estudiante.Crear(usuario.Id, anio: 1, grupo.CarreraId, fechaIngreso);
                db.Estudiantes.Add(estudiante);
                await db.SaveChangesAsync(ct);

                // InscripcionesMateria → Activa (se actualizarán al cerrar el año)
                foreach (var materiaId in materiaIds)
                    db.InscripcionesMateria.Add(InscripcionMateria.Crear(estudiante.Id, materiaId, cursoId));

                creados++;
            }

            await db.SaveChangesAsync(ct);
            logger.LogInformation("NuevosEstudiantes2022Seeder: C{C}{Com} — 30 alumnos creados.",
                grupo.CarreraId, grupo.Comision);
        }

        // Ajustar FechaInscripcion histórica
        var cursosIds = cursosPorCarreraComision.Values.ToList();
        var ids       = string.Join(",", cursosIds);
        await db.Database.ExecuteSqlRawAsync(
            $"UPDATE InscripcionesMateria SET FechaInscripcion = '2022-03-01' WHERE CursoId IN ({ids})",
            ct);

        logger.LogInformation("NuevosEstudiantes2022Seeder: {N} estudiantes cohorte 2022 creados.", creados);
    }
}
