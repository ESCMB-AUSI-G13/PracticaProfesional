using Microsoft.Extensions.Configuration;
using PracticaProfesional.Application.Auth.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Auth;

public class LoginUseCase(IUsuarioRepository usuarioRepository, IJwtService jwtService, IConfiguration configuration)
{
    public async Task<AuthResponseDto> EjecutarAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Credenciales inválidas.");

        if (!usuario.Activo)
            throw new UnauthorizedAccessException("La cuenta está desactivada.");

        if (!usuario.VerificarPassword(request.Password))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        var token = jwtService.GenerarToken(usuario);

        // Misma fuente que JwtService al firmar el token, para que el frontend no crea
        // que la sesión sigue viva después de que el JWT ya expiró (o viceversa).
        var expiracionHoras = int.Parse(configuration["Jwt:ExpirationHours"] ?? "8");

        return new AuthResponseDto(
            Token: token,
            Email: usuario.Email,
            NombreCompleto: $"{usuario.Nombre} {usuario.Apellido}",
            Rol: usuario.Rol.ToString(),
            Expiracion: DateTime.UtcNow.AddHours(expiracionHoras)
        );
    }
}
