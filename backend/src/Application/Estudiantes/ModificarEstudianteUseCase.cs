using PracticaProfesional.Application.Estudiantes.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Estudiantes;

public class ModificarEstudianteUseCase(
    IUsuarioRepository usuarioRepository,
    IEstudianteRepository estudianteRepository,
    ICarreraRepository carreraRepository,
    IMateriaRepository materiaRepository,
    IHistorialAcademicoRepository historialRepository,
    IInscripcionMateriaRepository inscripcionMateriaRepository,
    IAsistenciaRepository asistenciaRepository,
    IAuditoriaService auditoria)
{
    // Mismo umbral que ActualizarEstadoAcademicoUseCase.AniosInactividadDesercion.
    private const int AniosInactividadDesercion = 2;

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

        // Antes se podía forzar Egresado/Desertor desde este formulario genérico sin ningún
        // sustento real en el historial académico o la inactividad — bypaseaba por completo la
        // máquina de estados automática de CU-43 (ver CHECKLIST.md, Tier 5 #22). Mismos
        // criterios que ActualizarEstadoAcademicoUseCase, para que esto sea "confirmar
        // manualmente lo que ya debería ser cierto", no un override arbitrario.
        if (condicionDestino == CondicionEstudiante.Egresado)
            await ValidarPuedeEgresarAsync(estudiante, cancellationToken);
        else if (condicionDestino == CondicionEstudiante.Desertor)
            await ValidarPuedeDesertarAsync(estudiante, cancellationToken);

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

    private async Task ValidarPuedeEgresarAsync(Domain.Entities.Estudiante estudiante, CancellationToken ct)
    {
        var totalPlan = await materiaRepository.ContarPorCarreraIdAsync(estudiante.CarreraId, ct);
        var aprobados = await historialRepository.ContarAprobadosEnCarreraAsync(estudiante.Id, estudiante.CarreraId, ct);

        if (totalPlan == 0 || aprobados < totalPlan)
            throw new BusinessException(
                $"No se puede marcar Egresado: el estudiante completó {aprobados}/{totalPlan} materias del plan de su carrera.",
                409);
    }

    private async Task ValidarPuedeDesertarAsync(Domain.Entities.Estudiante estudiante, CancellationToken ct)
    {
        if (await inscripcionMateriaRepository.TieneAlgunaInscripcionActivaAsync(estudiante.Id, ct))
            throw new BusinessException(
                "No se puede marcar Desertor: el estudiante tiene inscripciones activas.", 409);

        var ultimaActividad = await asistenciaRepository.ObtenerUltimaFechaActividadAsync(estudiante.Id, ct);
        var fechaLimite = DateTime.UtcNow.AddYears(-AniosInactividadDesercion);

        if (ultimaActividad.HasValue && ultimaActividad.Value >= fechaLimite)
            throw new BusinessException(
                $"No se puede marcar Desertor: el estudiante tuvo actividad el {ultimaActividad.Value:yyyy-MM-dd}, " +
                $"no pasaron los {AniosInactividadDesercion} años de inactividad requeridos.",
                409);
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
