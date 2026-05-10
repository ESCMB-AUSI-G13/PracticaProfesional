using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Materias.DTOs;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Materias;

public class ModificarMateriaUseCase(
    IMateriaRepository materiaRepository,
    ICarreraRepository carreraRepository,
    IAuditoriaService auditoria)
{
    public async Task<MateriaDto> EjecutarAsync(int id, ModificarMateriaDto dto, CancellationToken cancellationToken = default)
    {
        var materia = await materiaRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró la materia con Id {id}.");

        var carrera = await carreraRepository.ObtenerPorIdAsync(dto.CarreraId, cancellationToken)
            ?? throw new BusinessException($"No se encontró la carrera con Id {dto.CarreraId}.");

        var anterior = new { materia.Nombre, materia.CarreraId, materia.Anio };

        materia.Modificar(dto.Nombre, dto.CarreraId, dto.Anio);
        await materiaRepository.GuardarCambiosAsync(cancellationToken);

        await auditoria.RegistrarAsync("Materia", materia.Id.ToString(), "MODIFICAR",
            valorAnterior: anterior,
            valorNuevo: new { materia.Nombre, materia.CarreraId, CarreraNombre = carrera.Nombre },
            cancellationToken: cancellationToken);

        return CrearMateriaUseCase.ToDto(materia, carrera.Nombre);
    }
}
