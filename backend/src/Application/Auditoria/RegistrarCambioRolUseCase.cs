using System.Security.Claims;
using PracticaProfesional.Application.Auditoria.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Auditoria;

public class RegistrarCambioRolUseCase(IAuditoriaRepository auditoriaRepository)
{
    private static readonly HashSet<string> AccionesValidas = ["ACTIVAR", "RESTAURAR"];

    public async Task EjecutarAsync(int usuarioId, RegistrarCambioRolDto dto, CancellationToken cancellationToken = default)
    {
        if (!AccionesValidas.Contains(dto.Accion))
            throw new ArgumentException($"Acción inválida: {dto.Accion}");

        var registro = AuditoriaCambioRol.Registrar(usuarioId, dto.RolOriginal, dto.RolVista, dto.Accion);
        await auditoriaRepository.RegistrarAsync(registro, cancellationToken);
    }
}
