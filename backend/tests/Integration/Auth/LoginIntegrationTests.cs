using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PracticaProfesional.Application.Auth.DTOs;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Tests.Integration.Auth;

public class LoginIntegrationTests : IClassFixture<WebAppFactory>
{
    private const string Email    = "admin.test@institucion.edu.ar";
    private const string Password = "Admin1234!";

    private readonly HttpClient _client;

    public LoginIntegrationTests(WebAppFactory factory)
    {
        _client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        if (!db.Usuarios.Any(u => u.Email == Email))
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(Password);
            db.Usuarios.Add(Usuario.Crear("11111111", "TSADM001", Email, "Admin", "Test", hash, Rol.Direccion));
            db.SaveChanges();
        }
    }

    [Fact]
    public async Task Login_ConCredencialesValidas_Devuelve200YToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestDto(Email, Password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
        Assert.Equal(Email, body.Email);
        Assert.Equal("Direccion", body.Rol);
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecta_Devuelve401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestDto(Email, "wrong-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ConEmailInexistente_Devuelve401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestDto("noexiste@test.com", Password));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
