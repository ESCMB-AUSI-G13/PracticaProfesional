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
        int? docenteId, int? anio, int? cursoId, int? carreraId, CancellationToken ct = default)
    {
        // 1. Cátedras con datos de navegación
        var espaciosQuery = db.EspaciosCurriculares
            .Include(ec => ec.Materia)
            .Include(ec => ec.Docente).ThenInclude(d => d.Usuario)
            .Include(ec => ec.Curso)
            .AsQueryable();

        if (docenteId.HasValue)  espaciosQuery = espaciosQuery.Where(ec => ec.DocenteId == docenteId.Value);
        if (anio.HasValue)       espaciosQuery = espaciosQuery.Where(ec => ec.Curso.Anio == anio.Value);
        if (cursoId.HasValue)    espaciosQuery = espaciosQuery.Where(ec => ec.CursoId == cursoId.Value);
        if (carreraId.HasValue)  espaciosQuery = espaciosQuery.Where(ec => ec.Materia.CarreraId == carreraId.Value);

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

        // Asistencias por (estudiante, año) — luego en memoria se usa solo el año más reciente por estudiante
        var asistenciasPorAnio = await db.Asistencias
            .Where(a => ids.Contains(a.EstudianteId))
            .GroupBy(a => new { a.EstudianteId, a.Fecha.Year })
            .Select(g => new
            {
                g.Key.EstudianteId,
                g.Key.Year,
                Total            = g.Count(),
                Ausencias        = g.Count(a => a.Estado != EstadoAsistencia.Presente),
                UltimaAsistencia = g.Max(a => a.Fecha)
            })
            .ToListAsync(ct);

        var asistenciaMap = asistenciasPorAnio
            .GroupBy(a => a.EstudianteId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var ultimo = g.OrderByDescending(a => a.Year).First();
                    return new
                    {
                        ultimo.Total,
                        ultimo.Ausencias,
                        UltimaAsistencia = g.Max(a => a.UltimaAsistencia)
                    };
                });

        // Notas por (estudiante, año) — se usa solo el año más reciente por estudiante
        var notasPorAnio = await (
            from ie in db.InscripcionesExamen
            join ex in db.Examenes on ie.ExamenId equals ex.Id
            where ids.Contains(ie.EstudianteId) && ie.NotaValor != null
            group new { ie.NotaValor, ex.FechaExamen.Year } by new { ie.EstudianteId, ex.FechaExamen.Year } into g
            select new
            {
                g.Key.EstudianteId,
                g.Key.Year,
                Promedio      = g.Average(x => x.NotaValor!.Value),
                Reprobadas    = g.Count(x => x.NotaValor < 4m),
                TotalExamenes = g.Count()
            }
        ).ToListAsync(ct);

        var notasMap = notasPorAnio
            .GroupBy(n => n.EstudianteId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(n => n.Year).First());

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
                TotalExamenes    = nota?.TotalExamenes ?? 0,
                UltimaAsistencia = (DateTime?)asis?.UltimaAsistencia
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

        // Paso 3: combinar — un registro por (estudiante × año historial)
        var resultado = new List<DatosRetencionAnualRawDto>();

        foreach (var e in estudiantes)
        {
            if (aniosPorEstudiante.TryGetValue(e.Id, out var anios))
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
    // Deserción por año de cursada (1°, 2°, 3°, 4°)
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<(List<(int AnioCursada, int Total, int Desertores)> Filas, int TotalGlobal, int DesertoresGlobal)> ObtenerDesercionPorAnioAsync(
        int? carreraId, int? anioCohorte, CancellationToken ct = default)
    {
        // Denominador por año: estudiantes DISTINTOS que cursaron cada nivel (via inscripciones)
        var inscBase = db.InscripcionesMateria
            .Join(db.Estudiantes, im => im.EstudianteId, e => e.Id,
                  (im, e) => new { im.EstudianteId, im.CursoId, e.CarreraId, e.FechaDeIngreso })
            .Join(db.Cursos, x => x.CursoId, c => c.Id,
                  (x, c) => new { x.EstudianteId, x.CarreraId, x.FechaDeIngreso, c.AnioLectivo });

        if (carreraId.HasValue)
            inscBase = inscBase.Where(x => x.CarreraId == carreraId.Value);

        if (anioCohorte.HasValue)
            inscBase = inscBase.Where(x => x.FechaDeIngreso.Year == anioCohorte.Value);

        var denominador = await inscBase
            .Select(x => new { x.EstudianteId, x.AnioLectivo })
            .Distinct()
            .GroupBy(x => x.AnioLectivo)
            .Select(g => new { AnioLectivo = g.Key, Total = g.Count() })
            .ToListAsync(ct);

        // Numerador por año: desertores agrupados por el año en que abandonaron
        var estBase = db.Estudiantes.AsQueryable();

        if (carreraId.HasValue)
            estBase = estBase.Where(e => e.CarreraId == carreraId.Value);

        if (anioCohorte.HasValue)
            estBase = estBase.Where(e => e.FechaDeIngreso.Year == anioCohorte.Value);

        var numerador = await estBase
            .Where(e => e.Condicion == CondicionEstudiante.Desertor)
            .GroupBy(e => e.Anio)
            .Select(g => new { Anio = g.Key, Desertores = g.Count() })
            .ToListAsync(ct);

        var desertoresPorAnio = numerador.ToDictionary(n => n.Anio, n => n.Desertores);

        var filas = denominador
            .Select(d => (
                AnioCursada: d.AnioLectivo,
                Total:       d.Total,
                Desertores:  desertoresPorAnio.GetValueOrDefault(d.AnioLectivo, 0)
            ))
            .OrderBy(r => r.AnioCursada)
            .ToList();

        // Totales globales reales: desde Estudiantes (sin doble conteo por años)
        int totalGlobal      = await estBase.CountAsync(ct);
        int desertoresGlobal = await estBase
            .CountAsync(e => e.Condicion == CondicionEstudiante.Desertor, ct);

        return (filas, totalGlobal, desertoresGlobal);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Egresados por carrera y cohorte
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<List<(string Carrera, int AnioCohorte, int TotalEgresados, int TotalAlumnos, double? DuracionPromedioAnios)>> ObtenerEgresadosPorCarreraAsync(
        int? carreraId, int? anioCohorte, CancellationToken ct = default)
    {
        // Base: todos los alumnos (para calcular total por cohorte)
        var baseQuery = db.Estudiantes.AsQueryable();
        if (carreraId.HasValue)
            baseQuery = baseQuery.Where(e => e.CarreraId == carreraId.Value);
        if (anioCohorte.HasValue)
            baseQuery = baseQuery.Where(e => e.FechaDeIngreso.Year == anioCohorte.Value);

        // Totales por cohorte (todos los alumnos, sin filtrar por condición)
        var totalesPorCohorte = await baseQuery
            .GroupBy(e => new { CarreraNombre = e.Carrera.Nombre, AnioCohorte = e.FechaDeIngreso.Year })
            .Select(g => new { g.Key.CarreraNombre, g.Key.AnioCohorte, Total = g.Count() })
            .ToListAsync(ct);

        // Egresados: traemos a memoria FechaDeIngreso y FechaDeEgreso para calcular duración en C#
        var egresadosRaw = await baseQuery
            .Where(e => e.Condicion == CondicionEstudiante.Egresado)
            .Select(e => new
            {
                CarreraNombre  = e.Carrera.Nombre,
                AnioCohorte    = e.FechaDeIngreso.Year,
                FechaIngreso   = e.FechaDeIngreso,
                FechaEgreso    = e.FechaDeEgreso
            })
            .ToListAsync(ct);

        var egresadosPorCohorte = egresadosRaw
            .GroupBy(e => new { e.CarreraNombre, e.AnioCohorte })
            .Select(g =>
            {
                var conDuracion = g.Where(x => x.FechaEgreso.HasValue).ToList();
                double? duracionPromedio = conDuracion.Count > 0
                    ? conDuracion.Average(x => (x.FechaEgreso!.Value - x.FechaIngreso).TotalDays) / 365.25
                    : null;
                return new
                {
                    g.Key.CarreraNombre,
                    g.Key.AnioCohorte,
                    TotalEgresados        = g.Count(),
                    DuracionPromedioAnios = duracionPromedio.HasValue
                        ? Math.Round(duracionPromedio.Value, 1)
                        : (double?)null
                };
            })
            .ToList();

        var result = totalesPorCohorte
            .Select(t =>
            {
                var eg = egresadosPorCohorte.FirstOrDefault(
                    e => e.CarreraNombre == t.CarreraNombre && e.AnioCohorte == t.AnioCohorte);
                return (
                    Carrera:              t.CarreraNombre,
                    AnioCohorte:          t.AnioCohorte,
                    TotalEgresados:       eg?.TotalEgresados ?? 0,
                    TotalAlumnos:         t.Total,
                    DuracionPromedioAnios: eg?.DuracionPromedioAnios
                );
            })
            .Where(r => r.TotalEgresados > 0)
            .OrderBy(r => r.Carrera)
            .ThenBy(r => r.AnioCohorte)
            .ToList();

        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Evolución de matrícula por año calendario
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<List<PuntoMatriculaDto>> ObtenerEvolucionMatriculaAsync(CancellationToken ct = default)
    {
        var anioActual = DateTime.UtcNow.Year;

        // Una sola query: atributos necesarios para reconstruir el rango de años activos por alumno
        var estudiantes = await db.Estudiantes
            .Select(e => new
            {
                AnioIngreso = e.FechaDeIngreso.Year,
                AnioEgreso  = e.FechaDeEgreso != null ? (int?)e.FechaDeEgreso.Value.Year : null,
                e.Condicion,
                AnioCarrera = (int)e.Anio   // año de carrera al momento de desertar (1, 2, 3, 4)
            })
            .ToListAsync(ct);

        int anioMinimo = estudiantes.Min(e => e.AnioIngreso);

        var puntos = new List<PuntoMatriculaDto>();

        for (int anio = anioMinimo; anio <= anioActual; anio++)
        {
            int totalActivos = 0;
            int ingresantes  = 0;

            foreach (var e in estudiantes)
            {
                if (e.AnioIngreso > anio) continue;

                // ¿Estaba activo este alumno en el año `anio`?
                bool activo = e.Condicion switch
                {
                    CondicionEstudiante.Regular
                    or CondicionEstudiante.Libre
                    or CondicionEstudiante.Promocional => true,
                    CondicionEstudiante.Egresado       => e.AnioEgreso == null || anio <= e.AnioEgreso.Value,
                    // Un desertor estuvo activo hasta el año calendario en que cursaba su AnioCarrera.
                    // Ej: ingresó en 2021, desertó en año 2 → último año activo = 2021 + (2-1) = 2022.
                    CondicionEstudiante.Desertor        => anio <= e.AnioIngreso + (e.AnioCarrera - 1),
                    _                                  => false
                };

                if (!activo) continue;

                totalActivos++;
                if (e.AnioIngreso == anio) ingresantes++;
            }

            puntos.Add(new PuntoMatriculaDto
            {
                Anio         = anio,
                TotalActivos = totalActivos,
                Ingresantes  = ingresantes,
                Continuantes = Math.Max(0, totalActivos - ingresantes)
            });
        }

        return puntos;
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
