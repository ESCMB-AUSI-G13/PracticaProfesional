using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Materias.DTOs;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Materias;

public class CrearMateriaUseCase(IMateriaRepository materiaRepository, IAuditoriaService auditoria)
{
    public async Task<MateriaDto> EjecutarAsync(CrearMateriaDto dto, CancellationToken cancellationToken = default)
    {
        if (await materiaRepository.ExistePorCodigoAsync(dto.Codigo, cancellationToken))
            throw new BusinessException($"Ya existe una materia con el código '{dto.Codigo.ToUpperInvariant()}'.");

        var materia = Materia.Crear(dto.Codigo, dto.Nombre, dto.Plan);
        await materiaRepository.AgregarAsync(materia, cancellationToken);

        await auditoria.RegistrarAsync("Materia", materia.Id.ToString(), "CREAR",
            valorAnterior: null,
            valorNuevo: new { materia.Codigo, materia.Nombre, materia.Plan },
            cancellationToken: cancellationToken);

        return ToDto(materia);
    }

    internal static MateriaDto ToDto(Materia m) => new(m.Id, m.Codigo, m.Nombre, m.Plan);
}
