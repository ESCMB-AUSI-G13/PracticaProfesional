using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.EstadoAcademico;
using PracticaProfesional.Application.EstadoAcademico.DTOs;

namespace PracticaProfesional.Controllers;

/// <summary>
/// Endpoints para la actualización automática del estado académico (CU-43).
/// </summary>
[ApiController]
[Route("api/estado-academico")]
[Authorize(Roles = "Direccion,Preceptor")]
public class EstadoAcademicoController(
    ActualizarEstadoAcademicoUseCase actualizarEstado) : ControllerBase
{
    /// <summary>
    /// Evalúa el rendimiento académico del estudiante y actualiza su estado de forma automática.
    ///
    /// Si se proveen <c>materiaId</c> y <c>cursoId</c>, se evalúan los criterios de
    /// asistencia, nota final, egreso y promoción para esa cursada.
    /// La deserción se evalúa siempre independientemente de los parámetros opcionales.
    /// </summary>
    [HttpPost("evaluar")]
    [ProducesResponseType(typeof(ResultadoActualizacionEstadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Evaluar(
        [FromBody] ActualizarEstadoAcademicoDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await actualizarEstado.EjecutarAsync(dto, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Evalúa únicamente el criterio de deserción para el estudiante indicado.
    /// Útil para procesos batch periódicos.
    /// </summary>
    [HttpPost("evaluar-desercion/{estudianteId:int}")]
    [ProducesResponseType(typeof(ResultadoActualizacionEstadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EvaluarDesercion(
        int estudianteId,
        CancellationToken cancellationToken)
    {
        var dto = new ActualizarEstadoAcademicoDto { EstudianteId = estudianteId };
        var resultado = await actualizarEstado.EjecutarAsync(dto, cancellationToken);
        return Ok(resultado);
    }
}
