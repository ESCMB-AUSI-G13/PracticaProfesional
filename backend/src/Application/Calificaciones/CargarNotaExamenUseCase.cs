using PracticaProfesional.Application.Calificaciones.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;
using PracticaProfesional.Domain.ValueObjects;

namespace PracticaProfesional.Application.Calificaciones;

/// <summary>
/// CU — Carga de calificaciones (Rol: Docente).
/// El docente registra la nota obtenida por un estudiante en un examen
/// (parcial escrito/oral, recuperatorio, final escrito/oral).
///
/// Reglas de negocio:
///   - La inscripción al examen debe estar en estado Activa.
///   - Si ya tiene nota cargada (estado Aprobada/Desaprobada) se rechaza la operación;
///     la rectificación requiere un proceso diferente con control de auditoría.
///   - La nota debe estar en el rango 1-10 (validado por el VO Nota).
///   - Todo cambio queda registrado de forma inmutable en Auditoria (CU-06).
/// </summary>
public class CargarNotaExamenUseCase(
    IInscripcionExamenRepository inscripcionExamenRepository,
    IAuditoriaService auditoria)
{
    public async Task<NotaExamenResultDto> EjecutarAsync(
        CargarNotaExamenDto dto,
        CancellationToken cancellationToken = default)
    {
        // 1. Obtener inscripción con datos del estudiante y del examen
        var inscripcion = await inscripcionExamenRepository
            .ObtenerPorIdAsync(dto.InscripcionExamenId, cancellationToken)
            ?? throw new BusinessException(
                $"No se encontró la inscripción a examen con Id {dto.InscripcionExamenId}.");

        // 2. Validar que la inscripción esté activa (no ya calificada ni dada de baja)
        if (inscripcion.Estado != EstadoInscripcion.Activa)
            throw new BusinessException(
                $"No se puede cargar la nota: la inscripción se encuentra en estado '{inscripcion.Estado}'. " +
                "Solo se permite cargar nota cuando el estado es Activa.");

        // 3. Capturar valor anterior para auditoría inmutable
        var valorAnterior = new
        {
            NotaValor = inscripcion.NotaValor,
            Estado = inscripcion.Estado.ToString()
        };

        // 4. Crear el Value Object Nota — valida rango 1-10 y redondea a 2 decimales
        var nota = Nota.Crear(dto.Nota);

        // 5. Delegar la lógica de transición de estado a la entidad de dominio
        inscripcion.CargarNota(nota);

        // 6. Persistir los cambios tracked por EF Core
        await inscripcionExamenRepository.GuardarCambiosAsync(cancellationToken);

        // 7. Registrar auditoría inmutable (CU-06)
        await auditoria.RegistrarAsync(
            entidadTipo: "InscripcionExamen",
            entidadId: inscripcion.Id.ToString(),
            accion: "CARGAR_NOTA",
            valorAnterior: valorAnterior,
            valorNuevo: new
            {
                NotaValor = inscripcion.NotaValor,
                Estado = inscripcion.Estado.ToString()
            },
            cancellationToken: cancellationToken);

        var estudiante = inscripcion.Estudiante;
        var examen = inscripcion.Examen;

        return new NotaExamenResultDto(
            InscripcionExamenId: inscripcion.Id,
            EstudianteId: inscripcion.EstudianteId,
            EstudianteNombreCompleto: $"{estudiante.Usuario.Nombre} {estudiante.Usuario.Apellido}",
            EstudianteLegajo: estudiante.Usuario.Legajo,
            ExamenId: inscripcion.ExamenId,
            TipoExamen: examen.TipoExamen.ToString(),
            MateriaNombre: examen.Materia.Nombre,
            NotaValor: nota.Valor,
            EsAprobado: nota.EsAprobado,
            Estado: inscripcion.Estado.ToString());
    }
}
