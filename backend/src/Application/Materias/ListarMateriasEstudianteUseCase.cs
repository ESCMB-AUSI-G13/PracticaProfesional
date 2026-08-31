using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Materias.DTOs;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Materias;

/// <summary>
/// CU-22: lista las materias en las que el estudiante autenticado puede inscribirse a cursar.
/// Filtra las materias del plan de su carrera que ya aprobó, en las que ya tiene una inscripción
/// activa, o cuyas correlatividades para cursar no cumple todavía.
/// </summary>
public class ListarMateriasEstudianteUseCase(
    IEstudianteRepository estudianteRepository,
    IMateriaRepository materiaRepository,
    ICorrelativiadadRepository correlativiadadRepository,
    IHistorialAcademicoRepository historialRepository,
    IInscripcionMateriaRepository inscripcionMateriaRepository)
{
    public async Task<IEnumerable<MateriaDto>> EjecutarAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el perfil de estudiante para el usuario {usuarioId}.");

        var materias = await materiaRepository.ListarPorCarreraIdAsync(estudiante.CarreraId, cancellationToken);
        var inscripcionesActivas = await inscripcionMateriaRepository.ListarActivasPorEstudianteAsync(estudiante.Id, cancellationToken);
        var materiaIdsConInscripcionActiva = inscripcionesActivas.Select(i => i.MateriaId).ToHashSet();

        // Traemos historial y correlatividades de TODA la carrera en dos consultas y evaluamos
        // en memoria — antes esto era 1 consulta por materia + 1 por cada correlatividad de cada
        // materia (N+1), lo que en un plan con varias materias/correlatividades hacía que esta
        // pantalla tardara varios segundos solo en round-trips a la base.
        var historial = (await historialRepository.ObtenerPorEstudianteAsync(estudiante.Id, cancellationToken)).ToList();
        var materiaIds = materias.Select(m => m.Id).ToList();
        var correlatividadesPorMateria = (await correlativiadadRepository
                .ObtenerParaCursarPorMateriasAsync(materiaIds, cancellationToken))
            .GroupBy(c => c.MateriaDestinoId)
            .ToDictionary(g => g.Key, g => g.ToList());

        bool EstaAprobado(int materiaId) =>
            historial.Any(h => h.MateriaId == materiaId && h.NotaFinal.HasValue && h.NotaFinal >= 4);

        bool EstaRegularizado(int materiaId) =>
            historial.Any(h => h.MateriaId == materiaId &&
                (h.Condicion == CondicionEstudiante.Regular || h.Condicion == CondicionEstudiante.Promocional));

        bool CumpleCorrelatividadesParaCursar(int materiaId)
        {
            if (!correlatividadesPorMateria.TryGetValue(materiaId, out var correlatividades))
                return true;

            return correlatividades.All(c => c.CondicionAcademica switch
            {
                CondicionAcademica.Regularizado => EstaRegularizado(c.MateriaRequisitoId),
                CondicionAcademica.Aprobado      => EstaAprobado(c.MateriaRequisitoId),
                _ => false
            });
        }

        var disponibles = new List<MateriaDto>();

        foreach (var materia in materias)
        {
            if (materia.Anio > estudiante.Anio)
                continue;

            if (materiaIdsConInscripcionActiva.Contains(materia.Id))
                continue;

            if (EstaAprobado(materia.Id))
                continue;

            if (!CumpleCorrelatividadesParaCursar(materia.Id))
                continue;

            disponibles.Add(materia);
        }

        return disponibles;
    }
}
