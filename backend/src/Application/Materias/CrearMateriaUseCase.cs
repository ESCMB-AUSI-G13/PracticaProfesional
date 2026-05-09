using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Materias.DTOs;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Materias;

public class CrearMateriaUseCase(
    IMateriaRepository materiaRepository,
    ICarreraRepository carreraRepository,
    IAuditoriaService auditoria)
{
    public async Task<MateriaDto> EjecutarAsync(CrearMateriaDto dto, CancellationToken cancellationToken = default)
    {
        var carrera = await carreraRepository.ObtenerPorIdAsync(dto.CarreraId, cancellationToken)
            ?? throw new BusinessException($"No se encontró la carrera con Id {dto.CarreraId}.");

        var numero = await materiaRepository.ObtenerSiguienteNumeroAsync(cancellationToken);
        var codigo = $"MAT-{numero:D3}";

        var materia = Materia.Crear(codigo, dto.Nombre, dto.CarreraId);
        await materiaRepository.AgregarAsync(materia, cancellationToken);

        await auditoria.RegistrarAsync("Materia", materia.Id.ToString(), "CREAR",
            valorAnterior: null,
            valorNuevo: new { materia.Codigo, materia.Nombre, CarreraId = dto.CarreraId, CarreraNombre = carrera.Nombre },
            cancellationToken: cancellationToken);

        return ToDto(materia, carrera.Nombre);
    }

    internal static MateriaDto ToDto(Materia m, string carreraNombre)
        => new(m.Id, m.Codigo, m.Nombre, m.CarreraId, carreraNombre);

    internal static MateriaDto ToDtoConNavegacion(Materia m)
        => new(m.Id, m.Codigo, m.Nombre, m.CarreraId, m.Carrera?.Nombre ?? string.Empty);
}
