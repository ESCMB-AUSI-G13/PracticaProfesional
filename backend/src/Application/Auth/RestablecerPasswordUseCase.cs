using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Auth;

public record RestablecerPasswordRequest(string Token, string NuevoPassword, string ConfirmarPassword);

public class RestablecerPasswordUseCase(IUsuarioRepository usuarioRepository)
{
    public async Task EjecutarAsync(RestablecerPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (request.NuevoPassword != request.ConfirmarPassword)
            throw new BusinessException("Las contraseñas no coinciden.");

        var usuario = await usuarioRepository.ObtenerPorTokenResetAsync(request.Token, cancellationToken);

        if (usuario is null || !usuario.EsTokenResetValido(request.Token))
            throw new BusinessException("El enlace de restablecimiento es inválido o ha expirado.");

        var nuevoHash = BCrypt.Net.BCrypt.HashPassword(request.NuevoPassword);
        usuario.RestablecerPassword(nuevoHash);

        await usuarioRepository.GuardarCambiosAsync(cancellationToken);
    }
}
