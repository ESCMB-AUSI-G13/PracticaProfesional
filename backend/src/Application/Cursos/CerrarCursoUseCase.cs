using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Cursos;

/// <summary>
/// Cierra un curso y liquida la cursada de cada alumno inscripto: decide si regularizó
/// (queda habilitado a rendir el examen final — CU-22/CU-33) o perdió la regularidad en
/// base a la asistencia, y lo deja registrado en HistorialAcademico. Sin este registro,
/// el motor de correlatividades y el conteo de egreso (CU-43) no tienen datos reales.
/// </summary>
public class CerrarCursoUseCase(
    ICursoRepository cursoRepository,
    IInscripcionMateriaRepository inscripcionMateriaRepository,
    IAsistenciaRepository asistenciaRepository,
    IHistorialAcademicoRepository historialRepository,
    IAuditoriaService auditoria,
    IUnitOfWork unitOfWork)
{
    // Mismo umbral que ActualizarEstadoAcademicoUseCase.AusenciaMaxRegularidad.
    // Si se cambia acá, cambiar también allá.
    private const decimal AusenciaMaxRegularidad = 0.25m;

    public async Task EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var curso = await cursoRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró el curso con Id {id}.");

        // Cierre + liquidación de cada inscripción + alta de historial deben confirmarse
        // juntos: antes, 3 SaveChanges independientes podían dejar el curso marcado Cerrado
        // con alumnos sin liquidar en HistorialAcademico si algo fallaba a mitad de camino
        // (ver CHECKLIST.md, Tier 2 #7). curso.Cerrar() también ahora rechaza cerrar un curso
        // ya cerrado (antes no validaba idempotencia).
        await unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            curso.Cerrar();
            await cursoRepository.GuardarCambiosAsync(cancellationToken);

            var inscripciones = await inscripcionMateriaRepository.ListarActivasPorCursoAsync(id, cancellationToken);

            var historiales = new List<HistorialAcademico>();
            foreach (var inscripcion in inscripciones)
            {
                var (total, ausentesInjust, _) = await asistenciaRepository.ObtenerEstadisticasAsync(
                    inscripcion.EstudianteId, inscripcion.MateriaId, inscripcion.CursoId, cancellationToken);

                var condicion = total > 0 && (decimal)ausentesInjust / total > AusenciaMaxRegularidad
                    ? CondicionEstudiante.Libre
                    : CondicionEstudiante.Regular;

                if (condicion == CondicionEstudiante.Libre)
                    inscripcion.MarcarDesaprobada();
                else
                    inscripcion.MarcarAprobada();

                historiales.Add(HistorialAcademico.Crear(
                    inscripcion.EstudianteId, inscripcion.MateriaId, inscripcion.CursoId,
                    curso.Anio, curso.Comision,
                    estadoFinal: condicion.ToString(), notaFinal: null, condicion));
            }

            await inscripcionMateriaRepository.GuardarCambiosAsync(cancellationToken);

            if (historiales.Count > 0)
                await historialRepository.AgregarRangoAsync(historiales, cancellationToken);

            await auditoria.RegistrarAsync("Curso", curso.Id.ToString(), "CERRAR",
                valorAnterior: new { Estado = "Activo" },
                valorNuevo: new { Estado = "Cerrado", AlumnosLiquidados = historiales.Count },
                cancellationToken: cancellationToken);
        }, cancellationToken: cancellationToken);
    }
}
