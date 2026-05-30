using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Genera cohortes históricas 2021-2024 con tasas académicas realistas.
///
/// Profesorado (4 años):
///   Año 1 → 45% deserta  | Retención: 55%
///   Año 2 → 25% deserta  | Retención: 75%
///   Año 3 → 25% deserta  | Retención: 75%
///   Año 4 → 10% deserta  | Egreso total: ~15% de la cohorte (5/30 por comisión)
///
/// Trayecto (2 años):
///   Año 1 → 33% deserta  | Retención: 67%
///   Año 2 → 10% deserta  | Retención: 90%
///   Egreso total: ~43% de la cohorte (13/30 por comisión)
///</summary>
public static class CohorteHistoricaSeeder
{
    private static readonly string[] Nombres =
    [
        "Alejandro", "Bárbara", "César", "Diana", "Eduardo", "Fernanda",
        "Gonzalo", "Hilda", "Ignacio", "Jimena", "Kevin", "Laura",
        "Marcelo", "Noemí", "Oscar", "Patricia", "Quintín", "Rosa",
        "Sergio", "Tatiana", "Ulises", "Valeria", "Walter", "Ximena",
        "Yago", "Zoe", "Adrián", "Betina", "Claudio", "Dolores",
        "Emilio", "Fiona", "Gastón", "Herminia", "Ivo", "Juliana",
        "Karina", "Luis", "Mariana", "Nahuel", "Ofelia", "Pablo",
        "Raúl", "Silvia", "Tirso", "Ursula", "Víctor", "Wendy",
        "Alexis", "Blanca"
    ];

    private static readonly string[] Apellidos =
    [
        "Alvarado", "Barrios", "Carrasco", "Delgado", "Espinoza", "Figueras",
        "Gallardo", "Huerta", "Iriarte", "Juárez", "Kramer", "Lamas",
        "Mansilla", "Núñez", "Oviedo", "Ponce", "Quintero", "Rossi",
        "Sosa", "Tapia", "Ugarte", "Vallejos", "Wenceslao", "Yáñez",
        "Zamudio", "Abrego", "Balmaceda", "Carmona", "Dorado", "Escudero",
        "Funes", "Garayo", "Huergo", "Izquierdo", "Jalil", "Lacunza",
        "Macías", "Nievas", "Olmedo", "Prieto", "Quirós", "Recalde",
        "Salazar", "Tobar", "Urquiza", "Villalba", "Windauer", "Xerez",
        "Zabala", "Piñeiro"
    ];

    // ── Configuración por grupo (comisión de 30 alumnos) ──────────────────────
    // DesertoresPorAnio[0] = desertores en año 1, [1] = año 2, [2] = año 3, etc.
    // El resto hasta 30 son Regulares (aún activos).
    private sealed record GrupoConfig(
        int Cohorte, int CarreraId, string Comision,
        int Egresados, int[] DesertoresPorAnio)
    {
        public int TotalDesertores => DesertoresPorAnio.Sum();
        public int Regulares       => 30 - Egresados - TotalDesertores;
    }

    // Profesorado (4 años) 30 alumnos/comisión:
    //   Año1→45%(13), Año2→25%(4), Año3→25%(3) → 20% egresa (6), 13% activos (4)
    // Trayecto (2 años) 30 alumnos/comisión:
    //   Año1→33%(10), Año2→12%(3)              → 43% egresa (13), 13% activos (4)
    private static readonly GrupoConfig[] Grupos =
    [
        // ── Profesorado 2021 — sin egresados (cohorte aún no completada), 13 activos intermitentes
        new(2021, 1, "A", Egresados: 0, DesertoresPorAnio: [10, 4, 3]),
        new(2021, 1, "B", Egresados: 0, DesertoresPorAnio: [10, 4, 3]),
        // ── Trayecto 2021 — egreso ~43% (13/30 por comisión) ─────────────────
        new(2021, 2, "A", Egresados: 13, DesertoresPorAnio: [10, 3]),
        new(2021, 2, "B", Egresados: 13, DesertoresPorAnio: [10, 3]),
        // ── Profesorado 2022 — sin egresados (cohorte aún no completada), 13 activos intermitentes
        new(2022, 1, "A", Egresados: 0, DesertoresPorAnio: [10, 4, 3]),
        new(2022, 1, "B", Egresados: 0, DesertoresPorAnio: [10, 4, 3]),
        // ── Trayecto 2022 — egreso ~43% (13/30 por comisión) ─────────────────
        new(2022, 2, "A", Egresados: 13, DesertoresPorAnio: [10, 3]),
        new(2022, 2, "B", Egresados: 13, DesertoresPorAnio: [10, 3]),
        // ── Trayecto 2023 — carrera completada en 2025, distribución final ────
        new(2023, 2, "A", Egresados: 13, DesertoresPorAnio: [10, 3]),
        new(2023, 2, "B", Egresados: 13, DesertoresPorAnio: [10, 3]),
        // ── Trayecto 2024 — carrera completada en 2026, distribución final ────
        new(2024, 2, "A", Egresados: 13, DesertoresPorAnio: [10, 3]),
        new(2024, 2, "B", Egresados: 13, DesertoresPorAnio: [10, 3]),
    ];

    // ─────────────────────────────────────────────────────────────────────────
    // SeedDesertoresActivosAsync — agrega los desertores históricos que faltan
    // para cohortes activas que solo tienen supervivientes en la BD.
    //
    // Profesorado 2024 (Año 3 en curso):  65 desd-Año1 + 25 desd-Año2 = 90 nuevos
    //   → Total: ~151, Desertores: ~90 (60%), Activos: ~61 (40%)
    //
    // Profesorado 2025 (Año 2 en curso):  49 desertores Año1
    //   → Total: ~109, Desertores: ~49 (45%), Activos: ~60 (55%)
    //
    // Trayecto 2025 (Año 2 en curso):     29 desertores Año1
    //   → Total: ~89, Desertores: ~29 (33%), Activos: ~60 (67%)
    //
    // Es idempotente: se omite si ya existen legajos con prefijo "EST-D".
    // ─────────────────────────────────────────────────────────────────────────
    public static async Task SeedDesertoresActivosAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // (carreraId, cohorte, prefijoBusqueda, grupos: (anioEnPrograma, cantidad)[])
        // El prefijoBusqueda se usa para idempotencia por cohorte.
        var configs = new (int CarreraId, int Cohorte, string Prefijo, (int Anio, int Cant)[] Grupos)[]
        {
            // Profesorado 2023 (Año 4 en curso): 60 activos en BD + 90 desertores históricos
            // → total ~150, deserción ~60 %
            (1, 2023, "EST-D2023-C1", [(1, 55), (2, 22), (3, 13)]),

            // Profesorado 2024 (Año 3 en curso): 61 activos + 90 desertores
            // → total ~151, deserción ~60 %
            (1, 2024, "EST-D2024-C1", [(1, 65), (2, 25)]),

            // Profesorado 2025 (Año 2 en curso): 60 activos + 49 desertores
            // → total ~109, deserción ~45 %
            (1, 2025, "EST-D2025-C1", [(1, 49)]),

            // Trayecto 2025 (Año 2 en curso): 60 activos + 29 desertores
            // → total ~89, deserción ~33 %
            (2, 2025, "EST-D2025-C2", [(1, 29)]),
        };

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Alumno2026!");
        int dniBase   = 42_000_001;
        int globalIdx = 0;
        int totalCreados = 0;

        foreach (var cfg in configs)
        {
            bool yaExiste = await db.Usuarios
                .AnyAsync(u => u.Legajo.StartsWith(cfg.Prefijo), ct);

            if (yaExiste)
            {
                logger.LogInformation(
                    "SeedDesertoresActivos: {P} ya existe, omitido.", cfg.Prefijo);
                continue;
            }

            var fechaIngreso = new DateTime(cfg.Cohorte, 3, 1);
            int idxEnCohorte = 0;

            foreach (var (anio, cant) in cfg.Grupos)
            {
                for (int i = 0; i < cant; i++, globalIdx++, idxEnCohorte++)
                {
                    var nombre   = Nombres[globalIdx % Nombres.Length];
                    var apellido = Apellidos[(globalIdx / Nombres.Length) % Apellidos.Length];
                    var legajo   = $"{cfg.Prefijo}-{idxEnCohorte + 1:D3}";
                    var email    = $"alu.d{cfg.Cohorte}.c{cfg.CarreraId}.{idxEnCohorte + 1:D3}@institucion.edu.ar";

                    var usuario = Usuario.Crear(
                        (dniBase++).ToString(), legajo, email,
                        nombre, apellido, passwordHash, Rol.Estudiante);
                    db.Usuarios.Add(usuario);
                    await db.SaveChangesAsync(ct);

                    var estudiante = Estudiante.Crear(usuario.Id, anio, cfg.CarreraId, fechaIngreso);
                    estudiante.Desertar();
                    db.Estudiantes.Add(estudiante);
                    await db.SaveChangesAsync(ct);

                    totalCreados++;
                }
            }

            logger.LogInformation(
                "SeedDesertoresActivos: {P} — {N} desertores creados.", cfg.Prefijo, idxEnCohorte);
        }

        logger.LogInformation("SeedDesertoresActivos: {T} desertores históricos creados en total.", totalCreados);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AsegurarDesertoresActivosAsync — garantiza que los estudiantes históricos
    // creados con prefijo EST-D (desertores de cohortes activas) conserven su
    // condición Desertor aunque CorregirCondicionesAsync los haya revertido.
    // Se ejecuta en Program.cs DESPUÉS de CorregirCondicionesAsync.
    // ─────────────────────────────────────────────────────────────────────────
    public static async Task AsegurarDesertoresActivosAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        int actualizados = await db.Database.ExecuteSqlRawAsync(
            """
            UPDATE e
            SET    e.Condicion = 'Desertor'
            FROM   Estudiantes e
            JOIN   Usuarios    u ON u.Id = e.UsuarioId
            WHERE  u.Legajo LIKE 'EST-D%'
              AND  e.Condicion != 'Desertor'
            """,
            ct);

        if (actualizados > 0)
            logger.LogInformation(
                "AsegurarDesertoresActivos: {N} estudiantes EST-D restablecidos a Desertor.", actualizados);
        else
            logger.LogInformation("AsegurarDesertoresActivos: todos los EST-D ya estaban como Desertor, sin cambios.");
    }

    private static int AnioMaxCarrera(int carreraId) => carreraId == 1 ? 4 : 2;

    // El egreso ocurre al final del año (cohorte + duración de carrera)
    private static (DateTime Desde, DateTime Hasta) RangoEgreso(int cohorte, int carreraId)
    {
        int anioEgreso = cohorte + AnioMaxCarrera(carreraId);
        return (new DateTime(anioEgreso, 10, 1), new DateTime(anioEgreso + 1, 3, 31));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RepararAsync — borra y recrea TODOS los estudiantes históricos.
    // Guard: si ya hay alumnos con la distribución correcta (Trayecto 2021 ≥ 24 egresados),
    // no hace nada.
    // ─────────────────────────────────────────────────────────────────────────
    public static async Task RepararAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        int egresadosC2_2021 = await db.Estudiantes
            .CountAsync(e => e.CarreraId == 2
                          && e.FechaDeIngreso.Year == 2021
                          && e.Condicion == CondicionEstudiante.Egresado, ct);

        // Trayecto 2024 debe tener 26 egresados (43% de 60); configuraciones viejas tenían solo 10.
        int egresadosC2_2024 = await db.Estudiantes
            .CountAsync(e => e.CarreraId == 2
                          && e.FechaDeIngreso.Year == 2024
                          && e.Condicion == CondicionEstudiante.Egresado, ct);

        // Profesorado 2021 debe tener 34 desertores (17×2) y 0 egresados.
        int desertoresC1_2021 = await db.Estudiantes
            .CountAsync(e => e.CarreraId == 1
                          && e.FechaDeIngreso.Year == 2021
                          && e.Condicion == CondicionEstudiante.Desertor, ct);

        int egresadosC1_2021 = await db.Estudiantes
            .CountAsync(e => e.CarreraId == 1
                          && e.FechaDeIngreso.Year == 2021
                          && e.Condicion == CondicionEstudiante.Egresado, ct);

        int desertoresHistoricos = await db.Estudiantes
            .CountAsync(e => e.FechaDeIngreso.Year <= 2022
                          && e.Condicion == CondicionEstudiante.Desertor, ct);

        // EST-H2023-C1 no deben existir (fueron creados por error en una versión anterior del seeder)
        bool prof2023HistoricoSobrante = await db.Usuarios
            .AnyAsync(u => u.Legajo.StartsWith("EST-H2023-C1"), ct);

        // Todo correcto: distribuciones 2021/2022/2024, sin egresados en C1-2021 y sin sobrantes accidentales
        if (egresadosC2_2021 >= 24 && egresadosC2_2024 >= 24
            && desertoresC1_2021 <= 34 && egresadosC1_2021 == 0
            && desertoresHistoricos >= 100
            && !prof2023HistoricoSobrante)
        {
            logger.LogInformation("CohorteHistoricaSeeder.Reparar: distribución ya correcta, omitido.");
            return;
        }

        logger.LogInformation("CohorteHistoricaSeeder.Reparar: iniciando limpieza y recreación...");

        // ── 1a. Eliminar sobrantes EST-H2023-C1 creados por error ───────────────
        var legajosH2023 = await db.Usuarios
            .Where(u => u.Legajo.StartsWith("EST-H2023-C1"))
            .Select(u => u.Id)
            .ToListAsync(ct);

        var estudiantesH2023 = await db.Estudiantes
            .Where(e => legajosH2023.Contains(e.UsuarioId))
            .Select(e => e.Id)
            .ToListAsync(ct);

        if (estudiantesH2023.Count > 0)
        {
            await db.Database.ExecuteSqlRawAsync(
                $"DELETE FROM HistorialAcademico WHERE EstudianteId IN ({string.Join(",", estudiantesH2023)})", ct);
            await db.Database.ExecuteSqlRawAsync(
                $"DELETE FROM Estudiantes WHERE Id IN ({string.Join(",", estudiantesH2023)})", ct);
        }
        if (legajosH2023.Count > 0)
            await db.Database.ExecuteSqlRawAsync(
                $"DELETE FROM Usuarios WHERE Id IN ({string.Join(",", legajosH2023)})", ct);

        logger.LogInformation(
            "CohorteHistoricaSeeder.Reparar: {E} sobrantes EST-H2023-C1 eliminados.", estudiantesH2023.Count);

        // ── 1b. Eliminar estudiantes históricos EST-H para recrearlos ────────
        var legajosHistoricos = await db.Usuarios
            .Where(u => u.Legajo.StartsWith("EST-H"))
            .Select(u => u.Id)
            .ToListAsync(ct);

        var estudiantesHistoricos = await db.Estudiantes
            .Where(e => legajosHistoricos.Contains(e.UsuarioId))
            .Select(e => e.Id)
            .ToListAsync(ct);

        if (estudiantesHistoricos.Count > 0)
        {
            await db.Database.ExecuteSqlRawAsync(
                $"DELETE FROM HistorialAcademico WHERE EstudianteId IN ({string.Join(",", estudiantesHistoricos)})",
                ct);
            await db.Database.ExecuteSqlRawAsync(
                $"DELETE FROM Estudiantes WHERE Id IN ({string.Join(",", estudiantesHistoricos)})",
                ct);
        }
        if (legajosHistoricos.Count > 0)
            await db.Database.ExecuteSqlRawAsync(
                $"DELETE FROM Usuarios WHERE Id IN ({string.Join(",", legajosHistoricos)})", ct);

        logger.LogInformation(
            "CohorteHistoricaSeeder.Reparar: {E} estudiantes y {U} usuarios históricos eliminados.",
            estudiantesHistoricos.Count, legajosHistoricos.Count);

        // ── 2. Recrear con la distribución correcta (incluye Profesorado 2023) ─
        await CrearEstudiantesAsync(db, logger, dniBase: 41_000_001, ct: ct);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SeedAsync — primera ejecución: crea si no existen cohortes históricas.
    // ─────────────────────────────────────────────────────────────────────────
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        bool yaExiste = await db.Estudiantes
            .AnyAsync(e => e.FechaDeIngreso.Year == 2021, ct);

        if (yaExiste)
        {
            logger.LogInformation("CohorteHistoricaSeeder: cohorte 2021 ya existe, omitido.");
            return;
        }

        await CrearEstudiantesAsync(db, logger, dniBase: 41_000_001, ct: ct);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CrearEstudiantesAsync — lógica común de creación de estudiantes.
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task CrearEstudiantesAsync(
        AppDbContext db, ILogger logger, int dniBase, CancellationToken ct)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Alumno2026!");
        var rng = new Random(77);
        int globalIdx = 0;
        int totalCreados = 0;

        foreach (var grupo in Grupos)
        {
            var fechaIngreso = new DateTime(grupo.Cohorte, 3, 1);
            var (egresoDesde, egresoHasta) = RangoEgreso(grupo.Cohorte, grupo.CarreraId);
            int egresoRango = Math.Max(1, (egresoHasta - egresoDesde).Days);

            var egresadosIds = new List<int>();

            // Construir lista ordenada de condiciones con año de deserción exacto
            var condicionesOrdenadas = BuildCondiciones(grupo);

            for (int i = 0; i < 30; i++, globalIdx++)
            {
                var (condicion, anioDesercion) = condicionesOrdenadas[i];

                int anioEstudiante = condicion == CondicionEstudiante.Desertor
                    ? anioDesercion
                    : AnioMaxCarrera(grupo.CarreraId);

                var nombre   = Nombres[globalIdx % Nombres.Length];
                var apellido = Apellidos[(globalIdx / Nombres.Length) % Apellidos.Length];
                var legajo   = $"EST-H{grupo.Cohorte}-C{grupo.CarreraId}{grupo.Comision}-{i + 1:D3}";
                var email    = $"alu.h{grupo.Cohorte}.c{grupo.CarreraId}{grupo.Comision.ToLower()}.{i + 1:D3}@institucion.edu.ar";

                var usuario = Usuario.Crear(
                    (dniBase++).ToString(), legajo, email,
                    nombre, apellido, passwordHash, Rol.Estudiante);
                db.Usuarios.Add(usuario);
                await db.SaveChangesAsync(ct);

                var estudiante = Estudiante.Crear(usuario.Id, anioEstudiante, grupo.CarreraId, fechaIngreso);
                switch (condicion)
                {
                    case CondicionEstudiante.Egresado: estudiante.Egresar();  break;
                    case CondicionEstudiante.Desertor: estudiante.Desertar(); break;
                }
                db.Estudiantes.Add(estudiante);
                await db.SaveChangesAsync(ct);

                if (condicion == CondicionEstudiante.Egresado)
                    egresadosIds.Add(estudiante.Id);

                totalCreados++;
            }

            // Actualizar FechaDeEgreso con fechas históricas realistas
            foreach (var id in egresadosIds)
            {
                var fecha = egresoDesde.AddDays(rng.Next(egresoRango));
                await db.Database.ExecuteSqlRawAsync(
                    "UPDATE Estudiantes SET FechaDeEgreso = {0} WHERE Id = {1}", fecha, id);
            }

            logger.LogInformation(
                "CohorteHistoricaSeeder: C{Car}{Com} {Coh} — {E}E / {D1}D-A1 / {D2}D-A2+ / {R}R.",
                grupo.CarreraId, grupo.Comision, grupo.Cohorte,
                grupo.Egresados,
                grupo.DesertoresPorAnio.Length > 0 ? grupo.DesertoresPorAnio[0] : 0,
                grupo.DesertoresPorAnio.Skip(1).Sum(),
                grupo.Regulares);
        }

        logger.LogInformation("CohorteHistoricaSeeder: {T} estudiantes históricos creados.", totalCreados);
    }

    // Construye la lista de (condición, añoDeserción) en el orden correcto:
    // Primero egresados, luego desertores por año (año1, año2, ...), luego regulares.
    private static List<(CondicionEstudiante Condicion, int AnioDesercion)> BuildCondiciones(GrupoConfig g)
    {
        var lista = new List<(CondicionEstudiante, int)>();

        for (int i = 0; i < g.Egresados; i++)
            lista.Add((CondicionEstudiante.Egresado, 0));

        for (int anioIdx = 0; anioIdx < g.DesertoresPorAnio.Length; anioIdx++)
            for (int j = 0; j < g.DesertoresPorAnio[anioIdx]; j++)
                lista.Add((CondicionEstudiante.Desertor, anioIdx + 1));

        for (int i = 0; i < g.Regulares; i++)
            lista.Add((CondicionEstudiante.Regular, 0));

        return lista;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SeedHistorialAsync — genera registros HistorialAcademico para que
    // los estudiantes históricos aparezcan como "continuantes" correctamente.
    //
    // Reglas:
    //   Egresado:          activo de ingreso+1 hasta FechaDeEgreso.Year
    //   Desertor año 1:    NO aparece como continuante (ingresó y abandonó)
    //   Desertor año N≥2:  aparece como continuante los N-1 años siguientes
    //   Regular/Libre:     activo de ingreso+1 hasta 2025
    // ─────────────────────────────────────────────────────────────────────────
    public static async Task SeedHistorialAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var cohortesCubiertas = new[] { 2021, 2022, 2023, 2024 };

        var idsHistoricos = await db.Estudiantes
            .Where(e => cohortesCubiertas.Contains(e.FechaDeIngreso.Year))
            .Select(e => e.Id)
            .ToListAsync(ct);

        if (idsHistoricos.Count == 0)
        {
            logger.LogWarning("CohorteHistoricaSeeder.Historial: sin estudiantes históricos, omitido.");
            return;
        }

        // Siempre limpiar y recrear para garantizar consistencia
        var existentes = await db.HistorialAcademico
            .Where(h => idsHistoricos.Contains(h.EstudianteId))
            .ToListAsync(ct);

        if (existentes.Count > 0)
        {
            db.HistorialAcademico.RemoveRange(existentes);
            await db.SaveChangesAsync(ct);
        }

        var primerCursoId   = await db.Cursos.Select(c => c.Id).FirstOrDefaultAsync(ct);
        var primerMateriaId = await db.Materias.Select(m => m.Id).FirstOrDefaultAsync(ct);
        if (primerCursoId == 0 || primerMateriaId == 0) return;

        var estudiantes = await db.Estudiantes
            .Where(e => idsHistoricos.Contains(e.Id))
            .Select(e => new { e.Id, e.Anio, e.Condicion, e.FechaDeIngreso, e.FechaDeEgreso, e.CarreraId })
            .ToListAsync(ct);

        var records = new List<HistorialAcademico>();

        foreach (var e in estudiantes)
        {
            int anioInicio = e.FechaDeIngreso.Year + 1;

            int anioFin = e.Condicion switch
            {
                // Egresado: activo hasta el año que egresó
                CondicionEstudiante.Egresado =>
                    e.FechaDeEgreso?.Year ?? (e.FechaDeIngreso.Year + AnioMaxCarrera(e.CarreraId)),

                // Desertor en año N: cursó N años
                //   Anio=1 → solo el año de ingreso, sin continuantes
                //   Anio=2 → continuante 1 año extra, etc.
                CondicionEstudiante.Desertor =>
                    e.FechaDeIngreso.Year + (e.Anio - 1),

                // Regular/Libre: activo hasta el último año con datos
                _ => 2025
            };

            for (int anio = anioInicio; anio <= Math.Min(anioFin, 2025); anio++)
            {
                records.Add(HistorialAcademico.Crear(
                    e.Id, primerMateriaId, primerCursoId,
                    anio, "A", "Aprobado", 6.0m, CondicionEstudiante.Regular));
            }
        }

        db.HistorialAcademico.AddRange(records);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "CohorteHistoricaSeeder.Historial: {N} registros para {E} estudiantes históricos.",
            records.Count, estudiantes.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CorregirDistribucionProf2023Async — ajusta in-place las condiciones de
    // los estudiantes EST-C1-4 (Profesorado 2023) para que queden coherentes
    // con la tasa histórica: ~36 desertores, ~4 egresados, ~20 activos.
    // Idempotente: no hace nada si ya hay ≥30 desertores en esa cohorte.
    // ─────────────────────────────────────────────────────────────────────────
    // Los desertores de la cohorte 2023 Profesorado se generan en SeedDesertoresActivosAsync
    // (registros sin asistencias, inmunes a CorregirCondicionesAsync).
    // Este método solo corrige los 4 egresados incorrectos (carrera de 4 años, aún en Año 4 en 2026).
    public static async Task CorregirDistribucionProf2023Async(
        AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        int egresadosActuales = await db.Estudiantes
            .CountAsync(e => e.CarreraId == 1
                          && e.FechaDeIngreso.Year == 2023
                          && e.Condicion == CondicionEstudiante.Egresado, ct);

        if (egresadosActuales == 0)
        {
            logger.LogInformation("CorregirDistribucionProf2023: sin egresados incorrectos, omitido.");
            return;
        }

        // Egresado es estado terminal en el dominio → resetear vía SQL directo.
        // La columna Condicion se almacena como nvarchar (nombre del enum), no como int.
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE Estudiantes SET Condicion = 'Regular' " +
            "WHERE CarreraId = 1 AND YEAR(FechaDeIngreso) = 2023 AND Condicion = 'Egresado'",
            ct);

        logger.LogInformation(
            "CorregirDistribucionProf2023: {E} egresados incorrectos reseteados a Regular.", egresadosActuales);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SeedInscripcionesCohorte2021Async
    //
    // Crea las InscripcionesMateria para los estudiantes de la cohorte 2021,
    // ligándolos a los cursos y materias de 1er año correspondientes.
    //
    // Estado de la inscripción:
    //   Desertor en año 1  → Baja       (abandonó en el primer año)
    //   Todos los demás    → Aprobada   (pasaron el año 1 y avanzaron)
    //
    // FechaInscripcion se ajusta a 2021-03-01 vía SQL para datos históricos correctos.
    // Idempotente: si ya existen inscripciones para cursos de 2021, se omite.
    // ─────────────────────────────────────────────────────────────────────────
    public static async Task SeedInscripcionesCohorte2021Async(
        AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // Guard
        var cursoIds2021 = await db.Cursos
            .Where(c => c.Anio == 2021)
            .Select(c => c.Id)
            .ToListAsync(ct);

        if (cursoIds2021.Count == 0)
        {
            logger.LogWarning("SeedInscripcionesCohorte2021: no hay cursos de 2021. Ejecutá CursosSeeder primero.");
            return;
        }

        bool yaExisten = await db.InscripcionesMateria
            .AnyAsync(im => cursoIds2021.Contains(im.CursoId), ct);

        if (yaExisten)
        {
            logger.LogInformation("SeedInscripcionesCohorte2021: inscripciones ya existen, omitido.");
            return;
        }

        // Materias de 1er año por carrera (CarreraId=1 Profesorado, CarreraId=2 Trayecto)
        var materiasPorCarrera = await db.Materias
            .Where(m => m.Anio == 1 && (m.CarreraId == 1 || m.CarreraId == 2))
            .GroupBy(m => m.CarreraId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(m => m.Id).ToArray(), ct);

        if (materiasPorCarrera.Count == 0)
        {
            logger.LogWarning("SeedInscripcionesCohorte2021: no se encontraron materias de 1er año.");
            return;
        }

        // Cursos de 2021 AnioLectivo=1, indexados por (CarreraId, Comision)
        var cursosPorCarreraComision = await db.Cursos
            .Where(c => c.Anio == 2021 && c.AnioLectivo == 1)
            .ToDictionaryAsync(c => (c.CarreraId, c.Comision), c => c.Id, ct);

        // Estudiantes cohorte 2021 con sus Usuarios
        var estudiantes = await db.Estudiantes
            .Include(e => e.Usuario)
            .Where(e => e.FechaDeIngreso.Year == 2021)
            .ToListAsync(ct);

        if (estudiantes.Count == 0)
        {
            logger.LogWarning("SeedInscripcionesCohorte2021: sin estudiantes de 2021. Ejecutá CohorteHistoricaSeeder primero.");
            return;
        }

        var nuevas = new List<InscripcionMateria>();

        foreach (var est in estudiantes)
        {
            // Parsear legajo: EST-H2021-C{carreraId}{comision}-{n}
            // Ejemplo: EST-H2021-C1A-001 → carreraId=1, comision="A"
            if (!TryParsarLegajoHistorico(est.Usuario.Legajo, out int carreraId, out string comision))
                continue;

            if (!cursosPorCarreraComision.TryGetValue((carreraId, comision), out int cursoId))
                continue;

            if (!materiasPorCarrera.TryGetValue(carreraId, out var materiaIds))
                continue;

            // Desertó en año 1 → Baja; el resto aprobó ese año
            bool desertorAnio1 = est.Condicion == CondicionEstudiante.Desertor && est.Anio == 1;

            foreach (var materiaId in materiaIds)
            {
                var insc = InscripcionMateria.Crear(est.Id, materiaId, cursoId);
                if (desertorAnio1)
                    insc.DarDeBaja();
                else
                    insc.MarcarAprobada();

                nuevas.Add(insc);
            }
        }

        db.InscripcionesMateria.AddRange(nuevas);
        await db.SaveChangesAsync(ct);

        // Corregir FechaInscripcion a la fecha real de inicio del ciclo 2021
        if (cursoIds2021.Count > 0)
        {
            var ids = string.Join(",", cursoIds2021);
            await db.Database.ExecuteSqlRawAsync(
                $"UPDATE InscripcionesMateria SET FechaInscripcion = '2021-03-01' WHERE CursoId IN ({ids})",
                ct);
        }

        logger.LogInformation(
            "SeedInscripcionesCohorte2021: {I} inscripciones creadas para {E} estudiantes.",
            nuevas.Count, estudiantes.Count);
    }

    // Parsea el legajo histórico: EST-H{año}-C{carreraId}{comision}-{n}
    private static bool TryParsarLegajoHistorico(string legajo, out int carreraId, out string comision)
    {
        carreraId = 0;
        comision  = string.Empty;

        // ["EST", "H2021", "C1A", "001"]
        var partes = legajo.Split('-');
        if (partes.Length < 4 || !partes[2].StartsWith('C'))
            return false;

        var carreraComision = partes[2][1..]; // "1A" o "2B"
        if (carreraComision.Length < 2)
            return false;

        // Todo menos el último char es el carreraId
        if (!int.TryParse(carreraComision[..^1], out carreraId))
            return false;

        comision = carreraComision[^1..].ToUpperInvariant();
        return true;
    }
}
