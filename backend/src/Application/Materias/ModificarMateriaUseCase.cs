using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Materias.DTOs;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Materias;

public class ModificarMateriaUseCase(IMateriaRepository materiaRepository, IAuditoriaService auditoria)
{
    public async Task<MateriaDto> EjecutarAsync(int id, ModificarMateriaDto dto, CancellationToken cancellationToken = default)
    {
        var materia = await materiaRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró la materia con Id {id}.");

        var anterior = new { materia.Nombre, materia.Plan };

        materia.Modificar(dto.Nombre, dto.Plan);
        await materiaRepository.GuardarCambiosAsync(cancellationToken);

        await auditoria.RegistrarAsync("Materia", materia.Id.ToString(), "MODIFICAR",
            valorAnterior: anterior,
            valorNuevo: new { materia.Nombre, materia.Plan },
            cancellationToken: cancellationToken);

        return CrearMateriaUseCase.ToDto(materia);
    }
}
