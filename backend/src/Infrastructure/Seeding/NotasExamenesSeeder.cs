using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.ValueObjects;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

public static class NotasExamenesSeeder
{
    // Distribución objetivo (deben sumar 1.0)
    private const double PctBaja        = 0.05; // ausente al examen → Estado Baja
    private const double PctLibre       = 0.25; // nota 1.00–3.99  → Estado Desaprobada → Condición Libre
    private const double PctPromocional = 0.20; // nota 7.00–10.00 → Estado Aprobada   → Condición Promocional
    // Regular = el resto (~50%): nota 4.00–6.99 → Estado Aprobada

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var totalActivas = await db.InscripcionesExamen
            .CountAsync(ie => ie.Estado == EstadoInscripcion.Activa, ct);

        if (totalActivas == 0)
        {
            logger.LogInformation("NotasExamenesSeeder: no hay inscripciones activas sin nota, seed omitido.");
            return;
        }

        var rng = new Random(42);

        var inscripciones = await db.InscripcionesExamen
            .Where(ie => ie.Estado == EstadoInscripcion.Activa)
            .Include(ie => ie.Estudiante)
            .ToListAsync(ct);

        // Un bucket por estudiante para que su condición sea coherente
        var porEstudiante = inscripciones
            .GroupBy(ie => ie.EstudianteId)
            .OrderBy(_ => rng.Next())
            .ToList();

        int total         = porEstudiante.Count;
        int limBaja       = (int)Math.Round(total * PctBaja);
        int limLibre      = limBaja + (int)Math.Round(total * PctLibre);
        int limPromocional = limLibre + (int)Math.Round(total * PctPromocional);

        int cBaja = 0, cLibre = 0, cRegular = 0, cPromocional = 0;

        for (int i = 0; i < porEstudiante.Count; i++)
        {
            var grupo      = porEstudiante[i];
            var estudiante = grupo.First().Estudiante;

            if (i < limBaja)
            {
                // Ausente: no rinde, se da de baja la inscripción
                foreach (var ie in grupo)
                    ie.DarDeBaja();
                cBaja++;
            }
            else if (i < limLibre)
            {
                // Libre: reprobó todos los parciales
                foreach (var ie in grupo)
                {
                    var valor = 1m + (decimal)(rng.NextDouble() * 2.99);
                    ie.CargarNota(Nota.Crear(valor));
                }
                SetCondicion(estudiante, CondicionEstudiante.Libre);
                cLibre++;
            }
            else if (i < limPromocional)
            {
                // Promocional: nota alta sin recuperatorio
                foreach (var ie in grupo)
                {
                    var valor = Math.Min(10m, 7m + (decimal)(rng.NextDouble() * 3.0));
                    ie.CargarNota(Nota.Crear(valor));
                }
                SetCondicion(estudiante, CondicionEstudiante.Promocional);
                cPromocional++;
            }
            else
            {
                // Regular: aprobó con nota media
                foreach (var ie in grupo)
                {
                    var valor = 4m + (decimal)(rng.NextDouble() * 2.99);
                    ie.CargarNota(Nota.Crear(valor));
                }
                SetCondicion(estudiante, CondicionEstudiante.Regular);
                cRegular++;
            }
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "NotasExamenesSeeder: {Total} estudiantes — Baja: {B}, Libre: {L}, Regular: {R}, Promocional: {P}.",
            total, cBaja, cLibre, cRegular, cPromocional);
    }

    // Navega la máquina de estados hasta el destino, independientemente del estado actual.
    private static void SetCondicion(
        PracticaProfesional.Domain.Entities.Estudiante e,
        CondicionEstudiante destino)
    {
        if (e.Condicion == destino) return;

        // Egresado es estado terminal — no se modifica
        if (e.Condicion == CondicionEstudiante.Egresado) return;

        // Desertor solo puede ir a Regular primero
        if (e.Condicion == CondicionEstudiante.Desertor)
            e.Reinscribir(); // → Regular

        if (e.Condicion == destino) return;

        switch (destino)
        {
            case CondicionEstudiante.Libre:
                // Regular → Libre  o  Promocional → Regular → Libre
                if (e.Condicion == CondicionEstudiante.Promocional)
                    e.RecuperarRegularidad();
                e.PerderRegularidad();
                break;

            case CondicionEstudiante.Promocional:
                // Regular → Promocional  o  Libre → Regular → Promocional
                if (e.Condicion == CondicionEstudiante.Libre)
                    e.RecuperarRegularidad();
                e.ObtenerPromocion();
                break;

            case CondicionEstudiante.Regular:
                // Libre → Regular  o  Promocional → Regular
                e.RecuperarRegularidad();
                break;
        }
    }
}
