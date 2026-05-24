using PracticaProfesional.Application.Alertas.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Alertas;

public class ListarAlertasUseCase(IAlertaRepository alertaRepo)
{
    public async Task<IEnumerable<AlertaDto>> EjecutarAsync(
        TipoAlerta? tipo = null,
        bool? enviada = null,
        CancellationToken cancellationToken = default)
    {
        var alertas = await alertaRepo.ListarAsync(tipo, enviada, cancellationToken);

        return alertas.Select(a => new AlertaDto
        {
            Id = a.Id,
            Tipo = a.Tipo.ToString(),
            Destinatario = a.Destinatario,
            Mensaje = a.Mensaje,
            Enviada = a.Enviada,
            FechaCreacion = a.FechaCreacion,
            EstudianteId = a.EstudianteId,
            NombreEstudiante = a.Estudiante != null
                ? $"{a.Estudiante.Usuario?.Nombre} {a.Estudiante.Usuario?.Apellido}"
                : null,
            CalendarioAcademicoId = a.CalendarioAcademicoId,
            NombreEvento = a.CalendarioAcademico?.NombreEvento
        });
    }
}
