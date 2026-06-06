using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PracticaProfesional.Application.Carreras.DTOs;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Tests.Integration.Carreras;

public class CarrerasIntegrationTests : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;

    public CarrerasIntegrationTests(WebAppFactory factory)
    {
        _client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        if (!db.Carreras.Any())
        {
            db.Carreras.Add(Carrera.Crear("Profesorado de Educación Secundaria en Economía", "Res. 001"));
            db.Carreras.Add(Carrera.Crear("Trayecto Pedagógico para Graduados No Docentes", "Res. 002"));
            db.SaveChanges();
        }
    }

    [Fact]
    public async Task Listar_SinAutenticacion_Devuelve200()
    {
        // GET /api/carreras es [AllowAnonymous]: debe responder 200 sin token
        var response = await _client.GetAsync("/api/carreras");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Listar_ConCarrerasSembradas_DevuelveListaNoVacia()
    {
        var response = await _client.GetAsync("/api/carreras");
        var carreras = await response.Content.ReadFromJsonAsync<List<CarreraDto>>();

        Assert.NotNull(carreras);
        Assert.NotEmpty(carreras);
    }
}
