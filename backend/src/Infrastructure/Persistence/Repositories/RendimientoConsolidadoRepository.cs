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

    // ─────────────────────────────────────────────────────────────────────────
    // Riesgo académico — datos crudos por estudiante activo
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<List<DatosRiesgoEstudianteDto>> ObtenerDatosRiesgoAsync(
        int? anioCohorte, int? carreraId, CancellationToken ct = default)
    {
        // Estudiantes activos (excluye Egresado y Desertor — ellos no tienen riesgo)
        var estudiantesQuery = db.Estudiantes
            .Where(e => e.Condicion != CondicionEstudiante.Egresado
                     && e.Condicion != CondicionEstudiante.Desertor);

        if (anioCohorte.HasValue)
            estudiantesQuery = estudiantesQuery
                .Where(e => e.FechaDeIngreso.Year == anioCohorte.Value);

        if (carreraId.HasValue)
            estudiantesQuery = estudiantesQuery
                .Where(e => e.CarreraId == carreraId.Value);

        var estudiantes = await estudiantesQuery
            .Select(e => new
            {
                e.Id,
                e.Anio,
                e.FechaDeIngreso,
                Condicion = e.Condicion.ToString(),
                e.Usuario.Legajo,
                e.Usuario.Nombre,
                e.Usuario.Apellido,
                CarreraNombre = e.Carrera.Nombre
            })
            .ToListAsync(ct);

        if (estudiantes.Count == 0)
            return [];

        var ids = estudiantes.Select(e => e.Id).ToList();

        // Asistencias agregadas por estudiante
        var asistencias = await db.Asistencias
            .Where(a => ids.Contains(a.EstudianteId))
            .GroupBy(a => a.EstudianteId)
            .Select(g => new
            {
                EstudianteId    = g.Key,
                Total           = g.Count(),
                Ausencias       = g.Count(a => a.Estado != EstadoAsistencia.Presente),
                UltimaAsistencia = g.Max(a => a.Fecha)
            })
            .ToListAsync(ct);

        var asistenciaMap = asistencias.ToDictionary(a => a.EstudianteId);

        // Notas de exámenes por estudiante
        var notas = await db.InscripcionesExamen
            .Where(ie => ids.Contains(ie.EstudianteId) && ie.NotaValor != null)
            .GroupBy(ie => ie.EstudianteId)
            .Select(g => new
            {
                EstudianteId = g.Key,
                Promedio     = g.Average(ie => ie.NotaValor!.Value),
                Reprobadas   = g.Count(ie => ie.NotaValor < 4m)
            })
            .ToListAsync(ct);

        var notasMap = notas.ToDictionary(n => n.EstudianteId);

        var hoy = DateTime.UtcNow.Date;

        return estudiantes.Select(e =>
        {
            asistenciaMap.TryGetValue(e.Id, out var asis);
            notasMap.TryGetValue(e.Id, out var nota);

            return new DatosRiesgoEstudianteDto
            {
                EstudianteId     = e.Id,
                Legajo           = e.Legajo,
                Nombre           = e.Nombre,
                Apellido         = e.Apellido,
                Carrera          = e.CarreraNombre,
                AnioCarrera      = e.Anio,
                AnioCohorte      = e.FechaDeIngreso.Year,
                Condicion        = e.Condicion,
                TotalClases      = asis?.Total ?? 0,
                Ausencias        = asis?.Ausencias ?? 0,
                PromedioNotas    = nota is not null ? (decimal?)nota.Promedio : null,
                Reprobadas       = nota?.Reprobadas ?? 0,
                UltimaAsistencia = asis?.UltimaAsistencia
            };
        }).ToList();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Años de cohorte disponibles (para deshabilitar opciones sin datos)
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<List<int>> ObtenerAniosCohorteAsync(int? carreraId, CancellationToken ct = default)
    {
        var query = db.Estudiantes.AsQueryable();

        if (carreraId.HasValue)
            query = query.Where(e => e.CarreraId == carreraId.Value);

        return await query
            .Select(e => e.FechaDeIngreso.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync(ct);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RR-12: Retención por año de cursada — datos crudos (estudiante × año historial)
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<List<DatosRetencionAnualRawDto>> ObtenerDatosRetencionAnualAsync(
        int? carreraId, int? anioCohorte, CancellationToken ct = default)
    {
        var estudiantesQuery = db.Estudiantes.AsQueryable();

        if (carreraId.HasValue)
            estudiantesQuery = estudiantesQuery.Where(e => e.CarreraId == carreraId.Value);

        if (anioCohorte.HasValue)
            estudiantesQuery = estudiantesQuery.Where(e => e.FechaDeIngreso.Year == anioCohorte.Value);

        // Paso 1: obtener todos los estudiantes con su cohorte y carrera
        var estudiantes = await estudiantesQuery
            .Select(e => new
            {
                e.Id,
                AnioCohorte  = e.FechaDeIngreso.Year,
                Carrera      = e.Carrera.Nombre,
                EsDesertor   = e.Condicion == CondicionEstudiante.Desertor
            })
            .ToListAsync(ct);

        if (estudiantes.Count == 0)
            return [];

        var ids = estudiantes.Select(e => e.Id).ToList();

        // Paso 2: pares (EstudianteId, Anio) distintos — unión de HistorialAcademico e InscripcionesMateria.
        // Un alumno cuenta como "retenido" en el año Y si tiene cualquier registro en alguna de las dos tablas.
        var desdeHistorial = await db.HistorialAcademico
            .Where(h => ids.Contains(h.EstudianteId))
            .Select(h => new { h.EstudianteId, h.Anio })
            .ToListAsync(ct);

        var desdeInscripciones = await db.InscripcionesMateria
            .Where(im => ids.Contains(im.EstudianteId))
            .Select(im => new { im.EstudianteId, Anio = im.Curso.Anio })
            .ToListAsync(ct);

        var aniosPorEstudiante = desdeHistorial
            .Concat(desdeInscripciones)
            .GroupBy(x => x.EstudianteId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Anio).Distinct().ToList());

        var historialLookup = aniosPorEstudiante;

        // Paso 3: combinar — un registro por (estudiante × año historial)
        var resultado = new List<DatosRetencionAnualRawDto>();

        foreach (var e in estudiantes)
        {
            if (historialLookup.TryGetValue(e.Id, out var anios))
            {
                foreach (var anio in anios)
                    resultado.Add(new DatosRetencionAnualRawDto
                    {
                        AnioCohorte   = e.AnioCohorte,
                        Carrera       = e.Carrera,
                        EstudianteId  = e.Id,
                        AnioHistorial = anio,
                        EsDesertor    = e.EsDesertor
                    });
            }
            else
            {
                resultado.Add(new DatosRetencionAnualRawDto
                {
                    AnioCohorte   = e.AnioCohorte,
                    Carrera       = e.Carrera,
                    EstudianteId  = e.Id,
                    AnioHistorial = -1,
                    EsDesertor    = e.EsDesertor
                });
            }
        }

        return resultado;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Retención por cohorte — agrupado por año de ingreso y carrera
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<List<DatosCohorteDto>> ObtenerDatosCohorteAsync(
        int? carreraId, int? anioCohorte, CancellationToken ct = default)
    {
        var query = db.Estudiantes.AsQueryable();

        if (carreraId.HasValue)
            query = query.Where(e => e.CarreraId == carreraId.Value);

        if (anioCohorte.HasValue)
            query = query.Where(e => e.FechaDeIngreso.Year == anioCohorte.Value);

        var raw = await query
            .Select(e => new
            {
                AnioCohorte  = e.FechaDeIngreso.Year,
                CarreraNombre = e.Carrera.Nombre,
                e.Condicion
            })
            .ToListAsync(ct);

        return raw
            .GroupBy(e => new { e.AnioCohorte, e.CarreraNombre })
            .Select(g =>
            {
                int activos   = g.Count(e => e.Condicion == CondicionEstudiante.Regular
                                          || e.Condicion == CondicionEstudiante.Libre
                                          || e.Condicion == CondicionEstudiante.Promocional);
                int egresados  = g.Count(e => e.Condicion == CondicionEstudiante.Egresado);
                int desertores = g.Count(e => e.Condicion == CondicionEstudiante.Desertor);

                return new DatosCohorteDto
                {
                    AnioCohorte = g.Key.AnioCohorte,
                    Carrera     = g.Key.CarreraNombre,
                    Total       = g.Count(),
                    Activos     = activos,
                    Egresados   = egresados,
                    Desertores  = desertores
                };
            })
            .OrderBy(d => d.AnioCohorte)
            .ThenBy(d => d.Carrera)
            .ToList();
    }
}
