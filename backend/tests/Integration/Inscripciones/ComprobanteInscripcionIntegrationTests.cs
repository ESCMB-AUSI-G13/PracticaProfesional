using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PracticaProfesional.Application.Auth.DTOs;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Tests.Integration.Inscripciones;

/// <summary>
/// Regresión del IDOR: GET /api/inscripciones/materias/{id}/comprobante devolvía DNI,
/// legajo y nombre de cualquier alumno a cualquier Estudiante autenticado con solo
/// cambiar el id en la URL. Ahora debe rechazar con 403 si el dueño de la inscripción no
/// es el usuario autenticado (salvo Dirección).
/// </summary>
public class ComprobanteInscripcionIntegrationTests : IClassFixture<WebAppFactory>
{
    private const string PasswordAlumnoA = "Alumno1234!";
    private const string PasswordAlumnoB = "Alumno5678!";
    private const string EmailAlumnoA = "alumno.a@institucion.edu.ar";
    private const string EmailAlumnoB = "alumno.b@institucion.edu.ar";

    private readonly WebAppFactory _factory;
    private readonly int _inscripcionIdDeA;

    public ComprobanteInscripcionIntegrationTests(WebAppFactory factory)
    {
        _factory = factory;

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        if (db.Usuarios.Any(u => u.Email == EmailAlumnoA))
        {
            _inscripcionIdDeA = db.InscripcionesMateria.Single(i => i.Estudiante.Usuario.Email == EmailAlumnoA).Id;
            return;
        }

        var carrera = Carrera.Crear("Profesorado en Matemática (IT)", "Res. IT-001");
        db.Carreras.Add(carrera);
        db.SaveChanges();

        var materia = Materia.Crear("MAT-IT", "Matemática I (IT)", carrera.Id, 1);
        db.Materias.Add(materia);
        db.SaveChanges();

        var usuarioPreceptor = Usuario.Crear("30000001", "PRE-IT-001", "preceptor.it@institucion.edu.ar", "Ana", "Preceptora", BCrypt.Net.BCrypt.HashPassword("x"), Rol.Preceptor);
        db.Usuarios.Add(usuarioPreceptor);
        db.SaveChanges();
        var preceptor = Preceptor.Crear(usuarioPreceptor.Id, "3810000000", "Mañana");
        db.Preceptores.Add(preceptor);
        db.SaveChanges();

        var curso = Curso.Crear(2024, 1, "IT-1A", cupo: 30, preceptor.Id, carrera.Id);
        db.Cursos.Add(curso);
        db.SaveChanges();

        var usuarioA = Usuario.Crear("30000002", "EST-IT-001", EmailAlumnoA, "Alumno", "A", BCrypt.Net.BCrypt.HashPassword(PasswordAlumnoA), Rol.Estudiante);
        var usuarioB = Usuario.Crear("30000003", "EST-IT-002", EmailAlumnoB, "Alumno", "B", BCrypt.Net.BCrypt.HashPassword(PasswordAlumnoB), Rol.Estudiante);
        db.Usuarios.AddRange(usuarioA, usuarioB);
        db.SaveChanges();

        var estudianteA = Estudiante.Crear(usuarioA.Id, 1, carrera.Id, DateTime.UtcNow.AddYears(-1));
        var estudianteB = Estudiante.Crear(usuarioB.Id, 1, carrera.Id, DateTime.UtcNow.AddYears(-1));
        db.Estudiantes.AddRange(estudianteA, estudianteB);
        db.SaveChanges();

        var inscripcionA = InscripcionMateria.Crear(estudianteA.Id, materia.Id, curso.Id);
        db.InscripcionesMateria.Add(inscripcionA);
        db.SaveChanges();

        _inscripcionIdDeA = inscripcionA.Id;
    }

    private async Task<string> LoginAsync(string email, string password)
    {
        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto(email, password));
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return auth!.Token;
    }

    [Fact]
    public async Task ObtenerComprobante_ComoDueñoDeLaInscripcion_Devuelve200()
    {
        var token = await LoginAsync(EmailAlumnoA, PasswordAlumnoA);
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/inscripciones/materias/{_inscripcionIdDeA}/comprobante");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ObtenerComprobante_DeOtroAlumno_Devuelve403()
    {
        var token = await LoginAsync(EmailAlumnoB, PasswordAlumnoB);
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/inscripciones/materias/{_inscripcionIdDeA}/comprobante");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
