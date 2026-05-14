using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class RendimientoConsolidadoRepository(AppDbContext db) : IRendimientoConsolidadoRepository
{
    // ─────────────────────────────────────────────────────────────────────────
    // RR-05: Comparativo de comisiones
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<IEnumerable<FilaComparativoComisionDto>> ObtenerComparativoComisionesAsync(
        int? materiaId, int? anio, int? docenteId, CancellationToken ct = default)
    {
        // IDs de materias que dicta el docente (si aplica el filtro)
        var materiaIdsDocente = docenteId.HasValue
            ? await db.EspaciosCurriculares
                .Where(ec => ec.DocenteId == docenteId.Value)
                .Select(ec => ec.MateriaId)
                .Distinct()
                .ToListAsync(ct)
            : null;

        // Total inscriptos por Materia+Curso (InscripcionesMateria)
        var totalesQuery = db.InscripcionesMateria
            .Join(db.Cursos, im => im.CursoId, c => c.Id,
                (im, c) => new { im.MateriaId, im.CursoId, c.Anio, c.Comision });

        if (materiaId.HasValue)
            totalesQuery = totalesQuery.Where(x => x.MateriaId == materiaId.Value);
        if (anio.HasValue)
            totalesQuery = totalesQuery.Where(x => x.Anio == anio.Value);
        if (materiaIdsDocente != null)
            totalesQuery = totalesQuery.Where(x => materiaIdsDocente.Contains(x.MateriaId));

        var totales = await totalesQuery
            .GroupBy(x => new { x.Anio, x.Comision })
            .Select(g => new { g.Key.Anio, g.Key.Comision, Total = g.Count() })
            .ToListAsync(ct);

        // Notas cargadas por Materia+Curso (InscripcionesExamen con NotaValor)
        var notasRaw = await (
            from ie in db.InscripcionesExamen
            where ie.NotaValor.HasValue
            join e in db.Examenes on ie.ExamenId equals e.Id
            join im in db.InscripcionesMateria
                on new { ie.EstudianteId, e.MateriaId }
                equals new { im.EstudianteId, im.MateriaId }
            join c in db.Cursos on im.CursoId equals c.Id
            where (!materiaId.HasValue || e.MateriaId == materiaId.Value)
               && (!anio.HasValue || c.Anio == anio.Value)
               && (materiaIdsDocente == null || materiaIdsDocente.Contains(e.MateriaId))
            select new { c.Anio, c.Comision, ie.NotaValor }
        ).ToListAsync(ct);

        // Merge en memoria
        var claves = totales
            .Select(t => new { t.Anio, t.Comision })
            .Union(notasRaw.Select(n => new { n.Anio, n.Comision }))
            .Distinct();

        return claves
            .Select(k =>
            {
                var totalInscriptos = totales
                    .FirstOrDefault(t => t.Anio == k.Anio && t.Comision == k.Comision)?.Total ?? 0;

                var notasComision = notasRaw
                    .Where(n => n.Anio == k.Anio && n.Comision == k.Comision)
                    .Select(n => n.NotaValor!.Value)
                    .ToList();

                var aprobados    = notasComision.Count(n => n >= 4);
                var desaprobados = notasComision.Count(n => n < 4);
                var totalConNota = notasComision.Count;
                var promedio     = totalConNota > 0 ? (decimal?)notasComision.Average() : null;
                var pct          = totalConNota > 0 ? aprobados * 100m / totalConNota : 0m;

                return new FilaComparativoComisionDto(
                    CursoAnio:            k.Anio,
                    Comision:             k.Comision,
                    TotalInscriptos:      totalInscriptos,
                    TotalConNota:         totalConNota,
                    Aprobados:            aprobados,
                    Desaprobados:         desaprobados,
                    PromedioGeneral:      promedio.HasValue ? Math.Round(promedio.Value, 2) : null,
                    PorcentajeAprobacion: Math.Round(pct, 2));
            })
            .OrderBy(f => f.CursoAnio)
            .ThenBy(f => f.Comision)
            .ToList();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RR-06: Evolución de notas en el tiempo (agrupado por año-mes de examen)
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<IEnumerable<PuntoEvolucionNotaDto>> ObtenerEvolucionNotasAsync(
        int? materiaId, int? anio, int? docenteId,
        int? cuatrimestre, byte? anioCarrera, TipoExamen? tipoExamen,
        string granularidad = "mensual",
        CancellationToken ct = default)
    {
        var materiaIdsDocente = docenteId.HasValue
            ? await db.EspaciosCurriculares
                .Where(ec => ec.DocenteId == docenteId.Value)
                .Select(ec => ec.MateriaId)
                .Distinct()
                .ToListAsync(ct)
            : null;

        var mesDesde = cuatrimestre.HasValue ? (cuatrimestre.Value == 1 ? 1 : 7)  : (int?)null;
        var mesHasta = cuatrimestre.HasValue ? (cuatrimestre.Value == 1 ? 6 : 12) : (int?)null;

        var notasRaw = await (
            from ie in db.InscripcionesExamen
            where ie.NotaValor.HasValue
            join e  in db.Examenes on ie.ExamenId equals e.Id
            join m  in db.Materias on e.MateriaId equals m.Id
            join c  in db.Carreras on m.CarreraId equals c.Id
            where (!materiaId.HasValue   || e.MateriaId        == materiaId.Value)
               && (!anio.HasValue        || e.FechaExamen.Year  == anio.Value)
               && (!mesDesde.HasValue    || (e.FechaExamen.Month >= mesDesde.Value && e.FechaExamen.Month <= mesHasta!.Value))
               && (!anioCarrera.HasValue || m.Anio              == anioCarrera.Value)
               && (!tipoExamen.HasValue  || e.TipoExamen        == tipoExamen.Value)
               && (materiaIdsDocente == null || materiaIdsDocente.Contains(e.MateriaId))
            select new {
                e.FechaExamen.Year,
                e.FechaExamen.Month,
                ie.NotaValor,
                CarreraId     = c.Id,
                CarreraNombre = c.Nombre,
            }
        ).ToListAsync(ct);

        // Proyectar cada nota con su período y clave de ordenamiento según granularidad
        var notasConClave = notasRaw.Select(n => new
        {
            n.NotaValor,
            n.CarreraId,
            n.CarreraNombre,
            Periodo = granularidad switch
            {
                "anual"         => $"{n.Year}",
                "cuatrimestral" => $"{n.Year}-C{(n.Month <= 6 ? 1 : 2)}",
                _               => $"{n.Year}-{n.Month:D2}",
            },
            SortKey = granularidad switch
            {
                "anual"         => n.Year * 100,
                "cuatrimestral" => n.Year * 100 + (n.Month <= 6 ? 1 : 2),
                _               => n.Year * 100 + n.Month,
            },
        });

        return notasConClave
            .GroupBy(n => new { n.Periodo, n.SortKey })
            .OrderBy(g => g.Key.SortKey)
            .Select(g =>
            {
                var notas        = g.Select(x => x.NotaValor!.Value).ToList();
                var aprobados    = notas.Count(n => n >= 4);
                var desaprobados = notas.Count(n => n < 4);
                var total        = notas.Count;
                var promedio     = total > 0 ? (decimal?)notas.Average() : null;
                var pct          = total > 0 ? aprobados * 100m / total : 0m;

                var porCarrera = g
                    .GroupBy(x => new { x.CarreraId, x.CarreraNombre })
                    .OrderBy(cg => cg.Key.CarreraId)
                    .Select(cg =>
                    {
                        var cn    = cg.Select(x => x.NotaValor!.Value).ToList();
                        var ca    = cn.Count(n => n >= 4);
                        var count = cn.Count;
                        return new DetalleCarreraEvolucionDto(
                            CarreraId:            cg.Key.CarreraId,
                            CarreraNombre:        cg.Key.CarreraNombre,
                            Promedio:             count > 0 ? (decimal?)Math.Round(cn.Average(), 2) : null,
                            PorcentajeAprobacion: count > 0 ? Math.Round(ca * 100m / count, 2) : 0m,
                            TotalEvaluados:       count);
                    })
                    .ToList();

                var distribucion = Enumerable.Range(1, 10)
                    .Select(n => new DistribucionNotaItemDto(n, notas.Count(x => (int)Math.Round(x) == n)))
                    .ToList();

                return new PuntoEvolucionNotaDto(
                    Periodo:              g.Key.Periodo,
                    TotalEvaluados:       total,
                    Aprobados:            aprobados,
                    Desaprobados:         desaprobados,
                    PromedioGeneral:      promedio.HasValue ? Math.Round(promedio.Value, 2) : null,
                    PorcentajeAprobacion: Math.Round(pct, 2),
                    PorCarrera:           porCarrera,
                    DistribucionNotas:    distribucion);
            })
            .ToList();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RR-07: Promedios por cátedra (EspacioCurricular)
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<IEnumerable<FilaPromedioCatedraDto>> ObtenerPromediosCatedraAsync(
        int? docenteId, int? anio, int? cursoId, CancellationToken ct = default)
    {
        // 1. Cátedras con datos de navegación
        var espaciosQuery = db.EspaciosCurriculares
            .Include(ec => ec.Materia)
            .Include(ec => ec.Docente).ThenInclude(d => d.Usuario)
            .Include(ec => ec.Curso)
            .AsQueryable();

        if (docenteId.HasValue) espaciosQuery = espaciosQuery.Where(ec => ec.DocenteId == docenteId.Value);
        if (anio.HasValue)      espaciosQuery = espaciosQuery.Where(ec => ec.Curso.Anio == anio.Value);
        if (cursoId.HasValue)   espaciosQuery = espaciosQuery.Where(ec => ec.CursoId == cursoId.Value);

        var espacios = await espaciosQuery.ToListAsync(ct);

        if (!espacios.Any())
            return [];

        var materiaIds = espacios.Select(ec => ec.MateriaId).Distinct().ToList();
        var cursoIds   = espacios.Select(ec => ec.CursoId).Distinct().ToList();

        // 2. Total estudiantes por Materia+Curso
        var totales = await db.InscripcionesMateria
            .Where(im => materiaIds.Contains(im.MateriaId) && cursoIds.Contains(im.CursoId))
            .GroupBy(im => new { im.MateriaId, im.CursoId })
            .Select(g => new { g.Key.MateriaId, g.Key.CursoId, Total = g.Count() })
            .ToListAsync(ct);

        // 3. Notas por Materia+Curso (via InscripcionExamen → Examen → InscripcionMateria)
        var notasRaw = await (
            from ie in db.InscripcionesExamen
            where ie.NotaValor.HasValue
            join e in db.Examenes on ie.ExamenId equals e.Id
            where materiaIds.Contains(e.MateriaId)
            join im in db.InscripcionesMateria
                on new { ie.EstudianteId, e.MateriaId }
                equals new { im.EstudianteId, im.MateriaId }
            where cursoIds.Contains(im.CursoId)
            select new { e.MateriaId, im.CursoId, ie.NotaValor }
        ).ToListAsync(ct);

        // 4. Proyectar por EspacioCurricular
        return espacios
            .Select(ec =>
            {
                var totalEst  = totales
                    .FirstOrDefault(t => t.MateriaId == ec.MateriaId && t.CursoId == ec.CursoId)?.Total ?? 0;

                var notas = notasRaw
                    .Where(n => n.MateriaId == ec.MateriaId && n.CursoId == ec.CursoId)
                    .Select(n => n.NotaValor!.Value)
                    .ToList();

                var aprobados    = notas.Count(n => n >= 4);
                var desaprobados = notas.Count(n => n < 4);
                var totalConNota = notas.Count;
                var promedio     = totalConNota > 0 ? (decimal?)notas.Average() : null;
                var pct          = totalConNota > 0 ? aprobados * 100m / totalConNota : 0m;

                return new FilaPromedioCatedraDto(
                    EspacioCurricularId:   ec.Id,
                    MateriaNombre:         ec.Materia.Nombre,
                    DocenteNombreCompleto: $"{ec.Docente.Usuario.Nombre} {ec.Docente.Usuario.Apellido}",
                    Comision:              ec.Curso.Comision,
                    CursoAnio:             ec.Curso.Anio,
                    TotalEstudiantes:      totalEst,
                    TotalConNota:          totalConNota,
                    Aprobados:             aprobados,
                    Desaprobados:          desaprobados,
                    PromedioGeneral:       promedio.HasValue ? Math.Round(promedio.Value, 2) : null,
                    PorcentajeAprobacion:  Math.Round(pct, 2));
            })
            .OrderBy(f => f.CursoAnio)
            .ThenBy(f => f.Comision)
            .ThenBy(f => f.MateriaNombre)
            .ToList();
    }

    // ─────────────────────────────────────────────────────────────────────────
    public async Task<string?> ObtenerNombreMateriaAsync(int materiaId, CancellationToken ct = default)
        => await db.Materias
            .Where(m => m.Id == materiaId)
            .Select(m => m.Nombre)
            .FirstOrDefaultAsync(ct);
}
