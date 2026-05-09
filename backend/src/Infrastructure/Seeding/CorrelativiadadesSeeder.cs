using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

public static class CorrelativiadadesSeeder
{
    // Resolución 0013 — Profesorado de Educación Secundaria en Economía
    private static readonly (string Destino, string Requisito, CondicionAcademica Condicion)[] ReglasCarrera1 =
    [
        ("Didáctica General",                    "Pedagogía",                                          CondicionAcademica.Regularizado),
        ("Práctica Docente II",                  "Problemáticas Socioantropológicas en Educación",     CondicionAcademica.Aprobado),
        ("Práctica Docente II",                  "Práctica Docente I",                                 CondicionAcademica.Regularizado),
        ("Microeconomía",                        "Introducción a la Economía",                         CondicionAcademica.Regularizado),
        ("Sist. de Info. Contable Superior",     "Sistema de Información Contable",                    CondicionAcademica.Regularizado),
        ("Matemática Financiera",                "Introducción a la Matemática",                       CondicionAcademica.Regularizado),
        ("Ciencias Económicas y su Didáctica I", "Introducción a la Economía",                         CondicionAcademica.Regularizado),
        ("Práctica Docente III",                 "Práctica Docente II",                                CondicionAcademica.Aprobado),
        ("Macroeconomía",                        "Cs. Económicas y su Didáctica I",                    CondicionAcademica.Regularizado),
        ("Macroeconomía",                        "Introducción a la Administración",                   CondicionAcademica.Regularizado),
        ("Macroeconomía",                        "Sistemas de Información Contable",                   CondicionAcademica.Regularizado),
        ("Administración y Organización",        "Introducción a la Economía",                         CondicionAcademica.Regularizado),
        ("Costos y Análisis de Est. Contables",  "Introducción a la Administración",                   CondicionAcademica.Regularizado),
        ("Ciencias Económicas y su Didáctica II","Sist. de Info. Contable Superior",                   CondicionAcademica.Regularizado),
        ("Historia del Pensamiento Económico",   "Sist. de Info. Contable Superior",                   CondicionAcademica.Regularizado),
        ("Historia del Pensamiento Económico",   "Cs. Económicas y su Didáctica I",                    CondicionAcademica.Regularizado),
        ("Historia del Pensamiento Económico",   "Introducción a la Administración",                   CondicionAcademica.Regularizado),
        ("Economía Financiera",                  "Historia Social y Económica Arg.",                   CondicionAcademica.Aprobado),
        ("Economía del Sector Público",          "Matemática Financiera",                              CondicionAcademica.Regularizado),
        ("Economía del Sector Público",          "Macroeconomía",                                      CondicionAcademica.Regularizado),
        ("Derecho Civil y Comercial",            "Macroeconomía",                                      CondicionAcademica.Regularizado),
        ("Derecho Laboral",                      "Introducción al Derecho",                            CondicionAcademica.Regularizado),
        ("Práctica IV",                          "Práctica Docente III",                               CondicionAcademica.Aprobado),
        ("Práctica IV",                          "Sujetos de la Educación",                            CondicionAcademica.Aprobado),
        ("Práctica IV",                          "Cs. Económicas y su Didáctica II",                   CondicionAcademica.Regularizado),
        ("Práctica IV",                          "Administración y Organización",                      CondicionAcademica.Regularizado),
        ("Práctica IV",                          "Macroeconomía",                                      CondicionAcademica.Regularizado),
        ("Práctica IV",                          "Costos y Análisis de Est. Contables",                CondicionAcademica.Regularizado),
    ];

    // Resolución 104/22 — Trayecto Pedagógico para Graduados No Docentes
    private static readonly (string Destino, string Requisito, CondicionAcademica Condicion)[] ReglasCarrera2 =
    [
        ("Sujetos y procesos de aprendizaje", "Perspectivas sobre la Educación",          CondicionAcademica.Regularizado),
        ("Espacio Orientado II",              "Espacio Orientado I",                       CondicionAcademica.Regularizado),
        ("Espacio Orientado III",             "Espacio Orientado I",                       CondicionAcademica.Regularizado),
        ("Proyectos Integrados",              "Bases Didácticas de la Ed. Secundaria",     CondicionAcademica.Regularizado),
        ("Proyectos Integrados",              "Saberes y Herramientas Digitales",          CondicionAcademica.Regularizado),
        ("Práctica II y Residencia",          "Práctica Docente I",                        CondicionAcademica.Aprobado),
        ("Práctica II y Residencia",          "Bases Didácticas de la Ed. Secundaria",     CondicionAcademica.Aprobado),
        ("Práctica II y Residencia",          "Educación Sexual Integral",                 CondicionAcademica.Regularizado),
    ];

    public static Task SeedCarrera1Async(AppDbContext db, ILogger logger, CancellationToken ct = default)
        => SeedReglasCursarAsync(db, logger, ReglasCarrera1, "Carrera 1 (Res. 0013)", ct);

    public static Task SeedCarrera2Async(AppDbContext db, ILogger logger, CancellationToken ct = default)
        => SeedReglasCursarAsync(db, logger, ReglasCarrera2, "Carrera 2 (Res. 104/22)", ct);

    private static async Task SeedReglasCursarAsync(
        AppDbContext db,
        ILogger logger,
        (string Destino, string Requisito, CondicionAcademica Condicion)[] reglas,
        string nombreCarrera,
        CancellationToken ct)
    {
        var materias = await db.Materias.AsNoTracking().ToListAsync(ct);

        if (materias.Count == 0)
        {
            logger.LogWarning("CorrelativiadadesSeeder [{Carrera}]: no hay materias en la BD. Seed omitido.", nombreCarrera);
            return;
        }

        var lookup = materias.ToDictionary(
            m => m.Nombre.Trim().ToUpperInvariant(),
            m => m.Id);

        var existentes = (await db.Correlatividades
            .AsNoTracking()
            .Select(c => new { c.MateriaDestinoId, c.MateriaRequisitoId, c.TipoRequerimiento })
            .ToListAsync(ct))
            .Select(e => (e.MateriaDestinoId, e.MateriaRequisitoId, e.TipoRequerimiento))
            .ToHashSet();

        int insertadas = 0, omitidas = 0, sinMateria = 0;

        foreach (var (destino, requisito, condicion) in reglas)
        {
            if (!lookup.TryGetValue(destino.Trim().ToUpperInvariant(), out var idDestino))
            {
                logger.LogWarning("CorrelativiadadesSeeder [{Carrera}]: materia destino no encontrada: '{Nombre}'", nombreCarrera, destino);
                sinMateria++;
                continue;
            }
            if (!lookup.TryGetValue(requisito.Trim().ToUpperInvariant(), out var idRequisito))
            {
                logger.LogWarning("CorrelativiadadesSeeder [{Carrera}]: materia requisito no encontrada: '{Nombre}'", nombreCarrera, requisito);
                sinMateria++;
                continue;
            }

            if (existentes.Contains((idDestino, idRequisito, "Cursar")))
            {
                omitidas++;
                continue;
            }

            db.Correlatividades.Add(Correlatividad.Crear(idDestino, idRequisito, "Cursar", condicion));
            existentes.Add((idDestino, idRequisito, "Cursar"));
            insertadas++;
        }

        if (insertadas > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "CorrelativiadadesSeeder [{Carrera}]: {I} insertadas, {O} ya existían, {S} con materia no encontrada.",
            nombreCarrera, insertadas, omitidas, sinMateria);
    }
}
