using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Carga los eventos del Calendario Académico 2026 basados en los cronogramas
/// del Trayecto (1° y 2° año A y B) y del Profesorado provistos por la institución.
/// Solo se ejecuta si la tabla está vacía.
/// </summary>
public static class CalendarioSeeder
{
    public static async Task SeedAsync(ICalendarioAcademicoRepository repo, CancellationToken ct = default)
    {
        if (await repo.TieneEventosAsync(ct)) return;

        var eventos = new List<CalendarioAcademico>
        {
            // ── TRAYECTO 2026 ──────────────────────────────────────────────────────
            Evento("Inicio de clases - Trayecto 2026", "Trayecto",
                new DateTime(2026, 4, 20), new DateTime(2026, 4, 20), TipoEvento.InicioClases),

            Evento("Período de inscripción a materias - Trayecto 1er cuatrimestre", "Trayecto",
                new DateTime(2026, 4, 13), new DateTime(2026, 4, 17), TipoEvento.InscripcionMateria),

            Evento("Período de inscripción a materias - Trayecto 2do cuatrimestre", "Trayecto",
                new DateTime(2026, 7, 27), new DateTime(2026, 8, 7), TipoEvento.InscripcionMateria),

            // ── PROFESORADO 2026 ───────────────────────────────────────────────────
            Evento("Inicio de clases - Profesorado 2026", "Profesorado",
                new DateTime(2026, 3, 16), new DateTime(2026, 3, 16), TipoEvento.InicioClases),

            Evento("Período de inscripción a materias - Profesorado 1er cuatrimestre", "Profesorado",
                new DateTime(2026, 3, 9),  new DateTime(2026, 3, 13), TipoEvento.InscripcionMateria),

            Evento("Período de inscripción a materias - Profesorado 2do cuatrimestre", "Profesorado",
                new DateTime(2026, 7, 27), new DateTime(2026, 8, 7), TipoEvento.InscripcionMateria),

            // ── RECESO (ambas carreras) ────────────────────────────────────────────
            Evento("Receso invernal 2026", "Todos",
                new DateTime(2026, 7, 6), new DateTime(2026, 7, 17), TipoEvento.Otro),

            // ── EXÁMENES 1er turno — Jul 20-31 (ambas carreras) ───────────────────
            Evento("Período de exámenes - 1er turno 2026", "Todos",
                new DateTime(2026, 7, 20), new DateTime(2026, 7, 31), TipoEvento.PeriodoExamen),

            Evento("Inscripción a exámenes - 1er turno 2026", "Todos",
                new DateTime(2026, 7, 13), new DateTime(2026, 7, 17), TipoEvento.InscripcionExamen),

            // ── EXÁMENES 2do turno — Nov 30 - Dic 18 (ambas carreras) ─────────────
            Evento("Período de exámenes - 2do turno 2026", "Todos",
                new DateTime(2026, 11, 30), new DateTime(2026, 12, 18), TipoEvento.PeriodoExamen),

            Evento("Inscripción a exámenes - 2do turno 2026", "Todos",
                new DateTime(2026, 11, 23), new DateTime(2026, 11, 27), TipoEvento.InscripcionExamen),

            // ── IEFI Trayecto ──────────────────────────────────────────────────────
            Evento("IEFI - Trayecto 1er cuatrimestre", "Trayecto",
                new DateTime(2026, 6, 29), new DateTime(2026, 7, 3), TipoEvento.Otro),

            Evento("IEFI - Trayecto 2do cuatrimestre", "Trayecto",
                new DateTime(2026, 11, 16), new DateTime(2026, 11, 27), TipoEvento.Otro),

            // ── IEFI Profesorado ───────────────────────────────────────────────────
            Evento("IEFI - Profesorado 2do cuatrimestre", "Profesorado",
                new DateTime(2026, 11, 9), new DateTime(2026, 11, 27), TipoEvento.Otro),

            // ── FIN DE CLASES ──────────────────────────────────────────────────────
            Evento("Fin de clases - Trayecto 2026", "Trayecto",
                new DateTime(2026, 11, 27), new DateTime(2026, 11, 27), TipoEvento.FinClases),

            Evento("Fin de clases - Profesorado 2026", "Profesorado",
                new DateTime(2026, 11, 7), new DateTime(2026, 11, 7), TipoEvento.FinClases),
        };

        foreach (var e in eventos)
            await repo.AgregarAsync(e, ct);
    }

    private static CalendarioAcademico Evento(
        string nombre, string comision, DateTime inicio, DateTime fin, TipoEvento tipo)
        => CalendarioAcademico.Crear(nombre, comision, inicio, fin, tipo);
}
