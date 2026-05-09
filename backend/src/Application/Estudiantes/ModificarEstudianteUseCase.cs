using PracticaProfesional.Application.Estudiantes.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Estudiantes;

public class ModificarEstudianteUseCase(
    IUsuarioRepository usuarioRepository,
    IEstudianteRepository estudianteRepository,
    ICarreraRepository carreraRepository,
    IAuditoriaService auditoria)
{
    public async Task<EstudianteDto> EjecutarAsync(int usuarioId, ModificarEstudianteDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Estudiante no encontrado.");

        var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Perfil de estudiante no encontrado.");

        if (await usuarioRepository.ExistePorEmailExcluyendoIdAsync(dto.Email, usuarioId, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        var carrera = await carreraRepository.ObtenerPorIdAsync(dto.CarreraId, cancellationToken)
            ?? throw new BusinessException($"No se encontró la carrera con Id {dto.CarreraId}.");

        if (!Enum.TryParse<CondicionEstudiante>(dto.Condicion, ignoreCase: true, out var condicionDestino))
            throw new ArgumentException($"Condición inválida: {dto.Condicion}");

        var anterior = new { usuario.Email, usuario.Nombre, usuario.Apellido, estudiante.Anio, estudiante.CarreraId, Condicion = estudiante.Condicion.ToString() };

        usuario.Modificar(dto.Nombre, dto.Apellido, dto.Email, usuario.Rol);
        estudiante.Modificar(dto.Anio, dto.CarreraId);
        AplicarTransicion(estudiante, condicionDestino);

        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoria.RegistrarAsync("Estudiante", estudiante.Id.ToString(), "MODIFICAR",
            valorAnterior: anterior,
            valorNuevo: new { usuario.Email, usuario.Nombre, usuario.Apellido, estudiante.Anio, estudiante.CarreraId, CarreraNombre = carrera.Nombre, Condicion = estudiante.Condicion.ToString() },
            cancellationToken);

        return CrearEstudianteUseCase.ToDto(estudiante, usuario, carrera.Nombre);
    }

    private static void AplicarTransicion(Domain.Entities.Estudiante estudiante, CondicionEstudiante destino)
    {
        switch (destino)
        {
            case CondicionEstudiante.Libre:       estudiante.PerderRegularidad();   break;
            case CondicionEstudiante.Promocional: estudiante.ObtenerPromocion();    break;
            case CondicionEstudiante.Regular:
                if (estudiante.Condicion == CondicionEstudiante.Desertor)
                    estudiante.Reinscribir();
                else
                    estudiante.RecuperarRegularidad();
                break;
            case CondicionEstudiante.Egresado:    estudiante.Egresar();             break;
            case CondicionEstudiante.Desertor:    estudiante.Desertar();            break;
        }
    }
}
