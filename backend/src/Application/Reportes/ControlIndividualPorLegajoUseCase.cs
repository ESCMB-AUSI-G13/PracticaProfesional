using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Reportes;

/// <summary>
/// RR-09: Control individual de asistencia por legajo (Nivel Operativo).
///
/// Devuelve el perfil del estudiante junto con el detalle de asistencia
/// por cada materia/curso cursada: totales, porcentajes y alertas de riesgo.
///
/// Umbrales de riesgo aplicados:
///   - En riesgo         : ausencias injustificadas > 20 % (zona de alerta)
///   - Regularidad perdida: ausencias injustificadas > 25 % (umbral de pérdida)
/// </summary>
public class ControlIndividualPorLegajoUseCase(
    IEstudianteRepository estudianteRepository,
    IAsistenciaRepository asistenciaRepository)
{
    private const decimal UmbralRiesgo    = 0.20m;
    private const decimal UmbralPerdida   = 0.25m;

    public async Task<ControlLegajoDto> EjecutarAsync(
        string legajo,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await estudianteRepository.ObtenerPorLegajoAsync(legajo, cancellationToken)
            ?? throw new BusinessException($"No se encontró ningún estudiante con legajo '{legajo}'.");

        var asistencias = (await asistenciaRepository.ObtenerPorEstudianteAsync(
            estudiante.Id, cancellationToken)).ToList();

        // Agrupar por (MateriaId, CursoId) para calcular estadísticas por cursada
        var resumenPorMateria = asistencias
            .GroupBy(a => new { a.MateriaId, a.CursoId })
            .Select(g =>
            {
                var primera        = g.First();
                int total          = g.Count();
                int presentes      = g.Count(a => a.Estado == Domain.Enums.EstadoAsistencia.Presente);
                int ausentesJust   = g.Count(a => a.Estado == Domain.Enums.EstadoAsistencia.AusenteJustificado);
                int ausentesInjust = g.Count(a => a.Estado == Domain.Enums.EstadoAsistencia.Ausente);

                decimal pctPresencia = total > 0
                    ? Math.Round((decimal)(presentes + ausentesJust) / total * 100, 2)
                    : 0m;
                decimal pctAusencia = total > 0
                    ? Math.Round((decimal)ausentesInjust / total * 100, 2)
                    : 0m;
                decimal tasaAusencia = total > 0 ? (decimal)ausentesInjust / total : 0m;

                return new ResumenAsistenciaMateriaDto
                {
                    MateriaId               = g.Key.MateriaId,
                    Materia                 = primera.Materia.Nombre,
                    Curso                   = $"{primera.Curso.Anio} – {primera.Curso.Comision}",
                    TotalClases             = total,
                    Presentes               = presentes,
                    AusentesJustificados    = ausentesJust,
                    AusentesInjustificados  = ausentesInjust,
                    PorcentajePresencia     = pctPresencia,
                    PorcentajeAusencias     = pctAusencia,
                    EnRiesgoRegularidad     = tasaAusencia >  UmbralRiesgo && tasaAusencia <= UmbralPerdida,
                    PerdioRegularidad       = tasaAusencia >  UmbralPerdida
                };
            })
            .OrderBy(r => r.Materia)
            .ToList();

        // Totales globales
        int totalGlobal      = resumenPorMateria.Sum(r => r.TotalClases);
        int presentesGlobal  = resumenPorMateria.Sum(r => r.Presentes);
        int ausjustGlobal    = resumenPorMateria.Sum(r => r.AusentesJustificados);
        int ausinjustGlobal  = resumenPorMateria.Sum(r => r.AusentesInjustificados);

        decimal pctGlobal = totalGlobal > 0
            ? Math.Round((decimal)(presentesGlobal + ausjustGlobal) / totalGlobal * 100, 2)
            : 0m;

        var usuario = estudiante.Usuario;

        return new ControlLegajoDto
        {
            EstudianteId                     = estudiante.Id,
            Legajo                           = usuario.Legajo,
            NombreCompleto                   = $"{usuario.Apellido}, {usuario.Nombre}",
            CondicionAcademica               = estudiante.Condicion.ToString(),
            Anio                             = estudiante.Anio,
            FechaDeIngreso                   = estudiante.FechaDeIngreso,
            AsistenciasPorMateria            = resumenPorMateria,
            TotalClasesGlobal                = totalGlobal,
            TotalPresentesGlobal             = presentesGlobal,
            TotalAusentesJustificadosGlobal  = ausjustGlobal,
            TotalAusentesInjustificadosGlobal = ausinjustGlobal,
            PorcentajePresenciaGlobal        = pctGlobal,
            MateriasEnRiesgo                 = resumenPorMateria.Count(r => r.EnRiesgoRegularidad),
            MateriasConRegularidadPerdida    = resumenPorMateria.Count(r => r.PerdioRegularidad),
            GeneradoEn                       = DateTime.UtcNow
        };
    }
}
