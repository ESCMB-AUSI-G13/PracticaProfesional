using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Auth;

public record RestablecerPasswordRequest(string Token, string NuevaPassword, string ConfirmarPassword);

public class RestablecerPasswordUseCase(IUsuarioRepository usuarioRepository)
{
    public async Task EjecutarAsync(RestablecerPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var token = request.Token?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(token))
            throw new BusinessException("El enlace de restablecimiento es inválido o ha expirado.");

        if (request.NuevaPassword != request.ConfirmarPassword)
            throw new BusinessException("Las contraseñas no coinciden.");

        var usuario = await usuarioRepository.ObtenerPorTokenResetAsync(token, cancellationToken);

        if (usuario is null || !usuario.EsTokenResetValido(token))
            throw new BusinessException("El enlace de restablecimiento es inválido o ha expirado.");

        var nuevoHash = BCrypt.Net.BCrypt.HashPassword(request.NuevaPassword);
        usuario.RestablecerPassword(nuevoHash);

        await usuarioRepository.GuardarCambiosAsync(cancellationToken);
    }
}
