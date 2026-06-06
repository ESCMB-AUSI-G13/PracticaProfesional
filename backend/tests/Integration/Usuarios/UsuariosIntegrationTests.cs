using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PracticaProfesional.Application.Auth.DTOs;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Tests.Integration.Usuarios;

public class UsuariosIntegrationTests : IClassFixture<WebAppFactory>
{
    private const string Email    = "direccion.test@institucion.edu.ar";
    private const string Password = "Admin1234!";

    private readonly WebAppFactory _factory;

    public UsuariosIntegrationTests(WebAppFactory factory)
    {
        _factory = factory;

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        if (!db.Usuarios.Any(u => u.Email == Email))
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(Password);
            db.Usuarios.Add(Usuario.Crear("22222222", "TSDIR001", Email, "Director", "Test", hash, Rol.Direccion));
            db.SaveChanges();
        }
    }

    [Fact]
    public async Task ListarUsuarios_SinToken_Devuelve401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/usuarios");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListarUsuarios_ConJwtDireccion_Devuelve200()
    {
        using var client = _factory.CreateClient();

        // Obtener JWT mediante login
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestDto(Email, Password));
        loginResponse.EnsureSuccessStatusCode();

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);

        var response = await client.GetAsync("/api/usuarios");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
