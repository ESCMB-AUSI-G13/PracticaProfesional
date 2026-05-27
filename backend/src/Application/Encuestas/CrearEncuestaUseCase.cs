using PracticaProfesional.Application.Encuestas.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Encuestas;

public class CrearEncuestaUseCase(IEncuestaRepository repo)
{
    public async Task<EncuestaDto> EjecutarAsync(CrearEncuestaDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo))
            throw new BusinessException("El título es obligatorio.");

        var encuesta = Encuesta.Crear(
            dto.Titulo, dto.Tipo, dto.CicloLectivo,
            dto.Descripcion, dto.MateriaId);

        await repo.AgregarAsync(encuesta, ct);
        return ListarEncuestasUseCase.ToDto(encuesta);
    }
}
