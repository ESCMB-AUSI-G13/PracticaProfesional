using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Materias;

public class EliminarMateriaUseCase(
    IMateriaRepository materiaRepository,
    IInscripcionMateriaRepository inscripcionRepository,
    IExamenRepository examenRepository,
    IHistorialAcademicoRepository historialRepository,
    IEspacioCurricularRepository espacioCurricularRepository,
    IAuditoriaService auditoria)
{
    public async Task EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var materia = await materiaRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró la materia con Id {id}.");

        var razones = new List<string>();

        if (await inscripcionRepository.ExistePorMateriaIdAsync(id, cancellationToken))
            razones.Add("tiene alumnos inscriptos");

        if (await examenRepository.ExistePorMateriaIdAsync(id, cancellationToken))
            razones.Add("tiene exámenes registrados");

        if (await historialRepository.ExistePorMateriaIdAsync(id, cancellationToken))
            razones.Add("tiene historial académico");

        // Sin este chequeo, borrar una materia con una cátedra (EspacioCurricular) asignada
        // rompía en el DELETE por violación de FK (FK_EspaciosCurriculares_Materias_MateriaId
        // es DeleteBehavior.Restrict) — ver CHECKLIST.md, Tier 4 #14.
        if (await espacioCurricularRepository.ExistePorMateriaIdAsync(id, cancellationToken))
            razones.Add("tiene cátedras asignadas");

        if (razones.Count > 0)
            throw new BusinessException(
                $"No se puede eliminar \"{materia.Nombre}\" porque {string.Join(", ", razones)}. " +
                "Primero dé de baja las inscripciones y registros asociados.");

        await auditoria.RegistrarAsync("Materia", id.ToString(), "ELIMINAR",
            valorAnterior: new { materia.Codigo, materia.Nombre, materia.CarreraId, materia.Anio },
            valorNuevo: null,
            cancellationToken: cancellationToken);

        await materiaRepository.EliminarAsync(id, cancellationToken);
    }
}
