using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class EncuestaRepository(AppDbContext db) : IEncuestaRepository
{
    public Task<List<Encuesta>> ListarActivasAsync(CancellationToken ct = default)
        => db.Encuestas
             .AsNoTracking()
             .Where(e => e.Activa)
             .Include(e => e.Preguntas.OrderBy(p => p.Orden))
             .OrderBy(e => e.CicloLectivo)
             .ToListAsync(ct);

    public Task<List<Encuesta>> ListarTodasAsync(CancellationToken ct = default)
        => db.Encuestas
             .AsNoTracking()
             .Include(e => e.Preguntas.OrderBy(p => p.Orden))
             .Include(e => e.Materia)
             .OrderByDescending(e => e.CicloLectivo)
             .ThenBy(e => e.Titulo)
             .ToListAsync(ct);

    public Task<Encuesta?> ObtenerConPreguntasAsync(int encuestaId, CancellationToken ct = default)
        => db.Encuestas
             .Include(e => e.Preguntas.OrderBy(p => p.Orden))
             .Include(e => e.Materia)
             .FirstOrDefaultAsync(e => e.Id == encuestaId, ct);

    public async Task AgregarAsync(Encuesta encuesta, CancellationToken ct = default)
    {
        await db.Encuestas.AddAsync(encuesta, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task GuardarCambiosAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);

    public async Task AgregarPreguntaAsync(PreguntaEncuesta pregunta, CancellationToken ct = default)
    {
        await db.PreguntasEncuesta.AddAsync(pregunta, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task AgregarRespuestaAsync(RespuestaEncuesta respuesta, CancellationToken ct = default)
    {
        await db.RespuestasEncuesta.AddAsync(respuesta, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> TokenYaExisteAsync(string tokenAnonimo, int encuestaId, CancellationToken ct = default)
        => db.EncuestasCompletadas
             .AnyAsync(ec => ec.TokenAnonimo == tokenAnonimo && ec.EncuestaId == encuestaId, ct);

    public async Task RegistrarCompletadaAsync(EncuestaCompletada completada, CancellationToken ct = default)
    {
        await db.EncuestasCompletadas.AddAsync(completada, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<List<RespuestaEncuesta>> ObtenerRespuestasConItemsAsync(
        int encuestaId, CancellationToken ct = default)
        => db.RespuestasEncuesta
             .AsNoTracking()
             .Where(r => r.EncuestaId == encuestaId)
             .Include(r => r.Items)
             .ToListAsync(ct);
}
