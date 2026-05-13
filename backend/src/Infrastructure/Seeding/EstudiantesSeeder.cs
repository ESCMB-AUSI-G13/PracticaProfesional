using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

public static class EstudiantesSeeder
{
    private static readonly string[] Nombres =
    [
        "Lucas", "Valentina", "Mateo", "Sofía", "Santiago", "Martina",
        "Tomás", "Camila", "Joaquín", "Lucía", "Nicolás", "Isabella",
        "Agustín", "Emma", "Facundo", "Julieta", "Ignacio", "Florencia",
        "Franco", "Carla", "Ezequiel", "Natalia", "Maximiliano", "Paula",
        "Rodrigo", "Antonella", "Bruno", "Victoria", "Lautaro", "Milagros",
        "Sebastián", "Candelaria", "Leandro", "Rocío", "Iván", "Yamila",
        "Cristian", "Aldana", "Germán", "Constanza", "Damián", "Nadia",
        "Ramiro", "Micaela", "Fernando", "Noelia", "Javier", "Brenda",
        "Hernán", "Melisa"
    ];

    private static readonly string[] Apellidos =
    [
        "González", "Rodríguez", "Gómez", "Fernández", "López",
        "Díaz", "Martínez", "Pérez", "García", "Torres",
        "Álvarez", "Romero", "Sánchez", "Ruiz", "Ramírez",
        "Flores", "Acosta", "Medina", "Herrera", "Castro",
        "Morales", "Ortiz", "Silva", "Vega", "Molina",
        "Ramos", "Cruz", "Suárez", "Reyes", "Gutiérrez",
        "Cabrera", "Pereyra", "Ibáñez", "Navarro", "Quiroga",
        "Leiva", "Salinas", "Correa", "Rivero", "Benítez",
        "Páez", "Giménez", "Segovia", "Cáceres", "Fuentes",
        "Mendoza", "Peralta", "Vera", "Ríos", "Vargas"
    ];

    // ── Calendario real 2026 ──────────────────────────────────────────────────
    // Profesorado (CarreraId = 1): 16-mar → 12-may  = 39 días hábiles
    //   Feriados excluidos: Jueves Santo 02-abr, Viernes Santo 03-abr, 01-may
    private static readonly DateTime[] FechasClaseProfesoral =
    [
        new(2026, 3, 16), new(2026, 3, 17), new(2026, 3, 18), new(2026, 3, 19), new(2026, 3, 20),
        new(2026, 3, 23), new(2026, 3, 24), new(2026, 3, 25), new(2026, 3, 26), new(2026, 3, 27),
        new(2026, 3, 30), new(2026, 3, 31),
        new(2026, 4, 1),
        new(2026, 4, 6),  new(2026, 4, 7),  new(2026, 4, 8),  new(2026, 4, 9),  new(2026, 4, 10),
        new(2026, 4, 13), new(2026, 4, 14), new(2026, 4, 15), new(2026, 4, 16), new(2026, 4, 17),
        new(2026, 4, 20), new(2026, 4, 21), new(2026, 4, 22), new(2026, 4, 23), new(2026, 4, 24),
        new(2026, 4, 27), new(2026, 4, 28), new(2026, 4, 29), new(2026, 4, 30),
        new(2026, 5, 4),  new(2026, 5, 5),  new(2026, 5, 6),  new(2026, 5, 7),  new(2026, 5, 8),
        new(2026, 5, 11), new(2026, 5, 12)
    ]; // 39 días

    // Trayecto (CarreraId = 2): 20-abr → 12-may  = 16 días hábiles
    //   Feriado excluido: 01-may
    private static readonly DateTime[] FechasClaseTrayecto =
    [
        new(2026, 4, 20), new(2026, 4, 21), new(2026, 4, 22), new(2026, 4, 23), new(2026, 4, 24),
        new(2026, 4, 27), new(2026, 4, 28), new(2026, 4, 29), new(2026, 4, 30),
        new(2026, 5, 4),  new(2026, 5, 5),  new(2026, 5, 6),  new(2026, 5, 7),  new(2026, 5, 8),
        new(2026, 5, 11), new(2026, 5, 12)
    ]; // 16 días

    private sealed record Grupo(
        int CarreraId,
        int AnioEstudiante,
        string Comision,
        int CursoId,
        int[] MateriaIds,
        int AnioIngreso,
        int CantLibre,
        int CantRegular,
        int CantPromocional
    );

    // 365 alumnos en total | 93 Libres + 198 Regulares + 74 Promocionales
    private static readonly Grupo[] Grupos =
    [
        //                                                         L    R   P
        new(1, 1, "A", 1,  [17,18,19,20,21,22,23,24,48],         2026, 10, 19,  3), // 32
        new(1, 1, "B", 2,  [17,18,19,20,21,22,23,24,48],         2026, 11, 17,  3), // 31
        new(1, 2, "A", 14, [25,26,27,28,29,30,31,49,50],         2025, 10, 15,  5), // 30
        new(1, 2, "B", 15, [25,26,27,28,29,30,31,49,50],         2025,  9, 15,  6), // 30
        new(1, 3, "A", 16, [32,33,34,35,36,37,38,39,51],         2024,  5, 17,  9), // 31
        new(1, 3, "B", 17, [32,33,34,35,36,37,38,39,51],         2024,  4, 17,  9), // 30
        new(1, 4, "A", 18, [40,41,42,43,44,45,46,47,52,53],      2023,  6, 18,  6), // 30
        new(1, 4, "B", 19, [40,41,42,43,44,45,46,47,52,53],      2023,  7, 17,  6), // 30
        new(2, 1, "A", 1,  [4,6,7,8,9,10,11],                    2026,  9, 16,  6), // 31
        new(2, 1, "B", 2,  [4,6,7,8,9,10,11],                    2026,  9, 16,  5), // 30
        new(2, 2, "A", 14, [12,13,14,15,16],                      2025,  6, 16,  8), // 30
        new(2, 2, "B", 15, [12,13,14,15,16],                      2025,  7, 15,  8), // 30
    ];

    // ────────────────────────────────────────────────────────────────────────────
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // Pre-carga mapa: legajo → { EstudianteId, Condicion }
        // Sirve tanto para detectar alumnos ya creados como para corregir condiciones.
        var mapaExistentes = await db.Usuarios
            .Where(u => u.Legajo.StartsWith("EST-"))
            .Join(db.Estudiantes,
                  u => u.Id,
                  e => e.UsuarioId,
                  (u, e) => new { u.Legajo, EstudianteId = e.Id, e.Condicion })
            .ToDictionaryAsync(x => x.Legajo, ct);

        // Pre-carga: EstudianteIds que ya tienen al menos un registro de asistencia
        var idsConAsistencias = (await db.Asistencias
            .Select(a => a.EstudianteId)
            .Distinct()
            .ToListAsync(ct))
            .ToHashSet();

        // Criterio de seed completo: ≥ 350 alumnos Y todos con asistencias
        bool estudiantesOk   = mapaExistentes.Count >= 350;
        bool asistenciasOk   = idsConAsistencias.Count >= mapaExistentes.Count - 5;

        if (estudiantesOk && asistenciasOk)
        {
            logger.LogInformation("EstudiantesSeeder: seed completo ({E} alumnos, {A} con asistencias), omitido.",
                mapaExistentes.Count, idsConAsistencias.Count);
            return;
        }

        logger.LogInformation(
            "EstudiantesSeeder: iniciando — {E} alumnos existentes, {A} con asistencias.",
            mapaExistentes.Count, idsConAsistencias.Count);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Alumno2026!");
        // DNI para alumnos verdaderamente nuevos (los 360 del seeder anterior ya
        // ocuparon 40_000_201 – 40_000_560).
        int dniNuevo     = 40_000_561;
        int creados      = 0;
        int corregidos   = 0;
        int conAsistNuev = 0;
        int globalIdx    = 0;

        foreach (var grupo in Grupos)
        {
            var fechasClase  = grupo.CarreraId == 1 ? FechasClaseProfesoral : FechasClaseTrayecto;
            int total        = grupo.CantLibre + grupo.CantRegular + grupo.CantPromocional;
            var fechaIngreso = new DateTime(grupo.AnioIngreso, 3, 1);
            var prefijo      = $"C{grupo.CarreraId}-{grupo.AnioEstudiante}{grupo.Comision}";

            for (int i = 0; i < total; i++, globalIdx++)
            {
                var condicionDeseada = DeterminarCondicion(i, grupo);
                bool desercionTemprana = condicionDeseada == CondicionEstudiante.Libre
                                         && i < grupo.CantLibre / 2;

                var legajo = $"EST-{prefijo}-{i + 1:D3}";
                int estudianteId;

                // ── Caso A: alumno ya existe ─────────────────────────────────
                if (mapaExistentes.TryGetValue(legajo, out var existente))
                {
                    estudianteId = existente.EstudianteId;

                    // Corregir condición si difiere (el seeder anterior creó todo como Regular)
                    if (existente.Condicion != condicionDeseada)
                    {
                        await CorregirCondicionAsync(db, existente.EstudianteId,
                            existente.Condicion, condicionDeseada, ct);
                        corregidos++;
                    }

                    // Ya tiene asistencias → nada más que hacer
                    if (idsConAsistencias.Contains(estudianteId)) continue;
                }
                // ── Caso B: alumno nuevo ─────────────────────────────────────
                else
                {
                    var nombre   = Nombres[globalIdx % Nombres.Length];
                    var apellido = Apellidos[(globalIdx / Nombres.Length) % Apellidos.Length];
                    var email    = $"alu.{prefijo.ToLower().Replace("-", ".")}.{i + 1:D3}@institucion.edu.ar";

                    var usuario = Usuario.Crear(
                        (dniNuevo++).ToString(), legajo, email,
                        nombre, apellido, passwordHash, Rol.Estudiante);
                    db.Usuarios.Add(usuario);
                    await db.SaveChangesAsync(ct);

                    var estudiante = Estudiante.Crear(
                        usuario.Id, grupo.AnioEstudiante, grupo.CarreraId, fechaIngreso);
                    AplicarCondicion(estudiante, condicionDeseada);
                    db.Estudiantes.Add(estudiante);
                    await db.SaveChangesAsync(ct);

                    foreach (var mid in grupo.MateriaIds)
                        db.InscripcionesMateria.Add(InscripcionMateria.Crear(estudiante.Id, mid, grupo.CursoId));

                    estudianteId = estudiante.Id;
                    creados++;
                }

                // ── Asistencias (tanto nuevos como existentes sin asistencias) ─
                foreach (var asistencia in GenerarAsistencias(
                    estudianteId, grupo.MateriaIds, grupo.CursoId,
                    fechasClase, condicionDeseada, desercionTemprana, globalIdx))
                {
                    db.Asistencias.Add(asistencia);
                }

                await db.SaveChangesAsync(ct);
                conAsistNuev++;
            }

            logger.LogInformation("EstudiantesSeeder: grupo {P} procesado.", prefijo);
        }

        logger.LogInformation(
            "EstudiantesSeeder: {C} alumnos creados, {Cor} condiciones corregidas, {A} asistencias generadas.",
            creados, corregidos, conAsistNuev);
    }

    // ── Corrige la condición académica de un estudiante ya existente ──────────
    // El seeder anterior creaba todos como Regular; este método los transiciona
    // a la condición correcta según la distribución real del grupo.
    private static async Task CorregirCondicionAsync(
        AppDbContext db,
        int estudianteId,
        CondicionEstudiante actual,
        CondicionEstudiante objetivo,
        CancellationToken ct)
    {
        var e = await db.Estudiantes.FindAsync(new object[] { estudianteId }, ct);
        if (e is null) return;

        try
        {
            if (actual == CondicionEstudiante.Regular && objetivo == CondicionEstudiante.Libre)
                e.PerderRegularidad();
            else if (actual == CondicionEstudiante.Regular && objetivo == CondicionEstudiante.Promocional)
                e.ObtenerPromocion();
            else if (actual == CondicionEstudiante.Libre && objetivo == CondicionEstudiante.Regular)
                e.RecuperarRegularidad();
            else if (actual == CondicionEstudiante.Promocional && objetivo == CondicionEstudiante.Regular)
                e.RecuperarRegularidad();

            await db.SaveChangesAsync(ct);
        }
        catch
        {
            // Transiciones inválidas en seed → ignorar silenciosamente
        }
    }

    private static CondicionEstudiante DeterminarCondicion(int idxEnGrupo, Grupo g)
    {
        if (idxEnGrupo < g.CantLibre)                         return CondicionEstudiante.Libre;
        if (idxEnGrupo < g.CantLibre + g.CantRegular)         return CondicionEstudiante.Regular;
        return CondicionEstudiante.Promocional;
    }

    private static void AplicarCondicion(Estudiante e, CondicionEstudiante condicion)
    {
        switch (condicion)
        {
            case CondicionEstudiante.Libre:       e.PerderRegularidad(); break;
            case CondicionEstudiante.Promocional: e.ObtenerPromocion();  break;
        }
    }

    // Genera registros de asistencia por cada (materia × día hábil).
    // Tasas de presencia según reglas de negocio:
    //   Promocional       : 82–100 %
    //   Regular           : 70– 81 %
    //   Libre desertor    :  0– 20 %
    //   Libre restante    : 20– 68 %
    private static IEnumerable<Asistencia> GenerarAsistencias(
        int estudianteId,
        int[] materiaIds,
        int cursoId,
        DateTime[] fechasClase,
        CondicionEstudiante condicion,
        bool desercionTemprana,
        int seed)
    {
        int totalDias = fechasClase.Length;
        var rng       = new Random(seed);

        double tasa = condicion switch
        {
            CondicionEstudiante.Promocional                    => 0.82 + rng.NextDouble() * 0.18,
            CondicionEstudiante.Regular                        => 0.70 + rng.NextDouble() * 0.11,
            CondicionEstudiante.Libre when desercionTemprana   => rng.NextDouble() * 0.20,
            _                                                  => 0.20 + rng.NextDouble() * 0.48
        };

        int diasPresente = Math.Clamp((int)Math.Round(tasa * totalDias), 0, totalDias);

        foreach (var materiaId in materiaIds)
        {
            var rngMat = new Random(seed * 997 + materiaId);
            var presentes = Enumerable.Range(0, totalDias)
                .OrderBy(_ => rngMat.Next())
                .Take(diasPresente)
                .ToHashSet();

            for (int d = 0; d < totalDias; d++)
            {
                yield return Asistencia.Registrar(
                    estudianteId, materiaId, cursoId,
                    fechasClase[d],
                    presentes.Contains(d) ? EstadoAsistencia.Presente : EstadoAsistencia.Ausente);
            }
        }
    }

    private static readonly string[] Motivos =
    [
        "Enfermedad",
        "Turno médico",
        "Enfermedad de familiar",
        "Motivo familiar",
        "Duelo",
        "Trámite administrativo",
        "Problema de transporte",
        "Paro de transporte público",
        "Trabajo",
        "Examen en otra materia",
        "Licencia por maternidad/paternidad",
        "Internación",
        "Viaje por motivo académico",
        "Condición climática extrema",
        "Consulta médica de urgencia"
    ];

    /// <summary>
    /// Convierte un porcentaje de ausencias existentes a AusenteJustificado con motivo realista.
    /// Proporciones: Promocional 30%, Regular 20%, Libre 8%.
    /// Es idempotente: se omite si ya existe más del 5% de justificadas.
    /// </summary>
    public static async Task PatchJustificacionesAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var rng = new Random(77);

        // Siempre corregir AusenteJustificado con Motivo nulo (sin importar el estado del resto)
        var sinMotivo = await db.Asistencias
            .Where(a => a.Estado == EstadoAsistencia.AusenteJustificado && a.Motivo == null)
            .ToListAsync(ct);

        foreach (var a in sinMotivo)
            a.Rectificar(EstadoAsistencia.AusenteJustificado, Motivos[rng.Next(Motivos.Length)]);

        if (sinMotivo.Count > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("PatchJustificaciones: {N} justificadas sin motivo corregidas.", sinMotivo.Count);
        }

        // Conversión de Ausente → AusenteJustificado (solo si aún no se hizo)
        var totalAusente      = await db.Asistencias.CountAsync(a => a.Estado == EstadoAsistencia.Ausente, ct);
        var totalJustificadas = await db.Asistencias.CountAsync(a => a.Estado == EstadoAsistencia.AusenteJustificado, ct);

        if (totalAusente == 0 || totalJustificadas > totalAusente * 0.05)
        {
            logger.LogInformation("PatchJustificaciones: conversión omitida ({J} justificadas ya existentes).", totalJustificadas);
            return;
        }

        logger.LogInformation("PatchJustificaciones: procesando justificaciones sobre {T} ausencias...", totalAusente);

        var condicionPorEstudiante = await db.Estudiantes
            .Select(e => new { e.Id, e.Condicion })
            .ToDictionaryAsync(e => e.Id, e => e.Condicion, ct);

        var ausencias = await db.Asistencias
            .Where(a => a.Estado == EstadoAsistencia.Ausente)
            .ToListAsync(ct);

        int total = 0;

        foreach (var grupo in ausencias.GroupBy(a => a.EstudianteId))
        {
            condicionPorEstudiante.TryGetValue(grupo.Key, out var condicion);
            double pct = condicion switch
            {
                CondicionEstudiante.Promocional => 0.30,
                CondicionEstudiante.Regular     => 0.20,
                _                               => 0.08
            };

            foreach (var a in grupo.OrderBy(_ => rng.Next()).Take((int)Math.Round(grupo.Count() * pct)))
            {
                a.Rectificar(EstadoAsistencia.AusenteJustificado, Motivos[rng.Next(Motivos.Length)]);
                total++;
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("PatchJustificaciones: {T} ausencias convertidas a justificadas.", total);
    }

    /// <summary>
    /// Corrige combinaciones nombre+apellido duplicadas entre alumnos del seed.
    /// </summary>
    public static async Task FixNombresAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var usuarios = await db.Usuarios
            .Where(u => u.Legajo.StartsWith("EST-"))
            .OrderBy(u => u.Id)
            .Select(u => new { u.Id, u.Nombre, u.Apellido })
            .ToListAsync(ct);

        if (usuarios.Count == 0) return;

        bool hayDuplicados = usuarios.GroupBy(u => (u.Nombre, u.Apellido)).Any(g => g.Count() > 1);
        if (!hayDuplicados)
        {
            logger.LogInformation("EstudiantesSeeder.FixNombres: sin duplicados.");
            return;
        }

        logger.LogInformation("EstudiantesSeeder.FixNombres: corrigiendo {N} alumnos...", usuarios.Count);
        int actualizados = 0;
        for (int idx = 0; idx < usuarios.Count; idx++)
        {
            var nombre   = Nombres[idx % Nombres.Length];
            var apellido = Apellidos[(idx / Nombres.Length) % Apellidos.Length];
            if (usuarios[idx].Nombre == nombre && usuarios[idx].Apellido == apellido) continue;
            await db.Database.ExecuteSqlRawAsync(
                "UPDATE Usuarios SET Nombre = {0}, Apellido = {1} WHERE Id = {2}",
                nombre, apellido, usuarios[idx].Id);
            actualizados++;
        }
        logger.LogInformation("EstudiantesSeeder.FixNombres: {A} registros actualizados.", actualizados);
    }
}
