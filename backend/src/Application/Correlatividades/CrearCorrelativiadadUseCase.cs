using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Correlatividades;

public record CrearCorrelativiadadDto(
    int MateriaDestinoId,
    int MateriaRequisitoId,
    string TipoRequerimiento,   // "Cursar" | "Rendir"
    CondicionAcademica CondicionAcademica);

public record CorrelativiadadDto(
    int Id,
    int MateriaDestinoId,
    int MateriaRequisitoId,
    string NombreRequisito,
    string TipoRequerimiento,
    string CondicionAcademica);

/// <summary>
/// Crea una correlatividad validando que no se genere un ciclo en el grafo de dependencias.
/// Un ciclo ocurre cuando agregar (Destino=A, Requisito=B) crea el camino B →...→ A → B.
/// </summary>
public class CrearCorrelativiadadUseCase(
    ICorrelativiadadRepository correlativiadadRepository,
    IMateriaRepository materiaRepository)
{
    public async Task<CorrelativiadadDto> EjecutarAsync(
        CrearCorrelativiadadDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.TipoRequerimiento) ||
            (dto.TipoRequerimiento != "Cursar" && dto.TipoRequerimiento != "Rendir"))
            throw new BusinessException("TipoRequerimiento debe ser 'Cursar' o 'Rendir'.");

        _ = await materiaRepository.ObtenerPorIdAsync(dto.MateriaDestinoId, cancellationToken)
            ?? throw new BusinessException($"No se encontró la materia destino con Id {dto.MateriaDestinoId}.");
        _ = await materiaRepository.ObtenerPorIdAsync(dto.MateriaRequisitoId, cancellationToken)
            ?? throw new BusinessException($"No se encontró la materia requisito con Id {dto.MateriaRequisitoId}.");

        await ValidarSinCicloAsync(dto.MateriaDestinoId, dto.MateriaRequisitoId, dto.TipoRequerimiento, cancellationToken);

        var correlatividad = Correlatividad.Crear(
            dto.MateriaDestinoId,
            dto.MateriaRequisitoId,
            dto.TipoRequerimiento,
            dto.CondicionAcademica);

        await correlativiadadRepository.AgregarAsync(correlatividad, cancellationToken);

        var nombreRequisito = (await materiaRepository.ObtenerPorIdAsync(dto.MateriaRequisitoId, cancellationToken))!.Nombre;
        return new CorrelativiadadDto(
            correlatividad.Id,
            correlatividad.MateriaDestinoId,
            correlatividad.MateriaRequisitoId,
            nombreRequisito,
            correlatividad.TipoRequerimiento,
            correlatividad.CondicionAcademica.ToString());
    }

    // ── Detección de ciclos (DFS) ────────────────────────────────────────────────

    private async Task ValidarSinCicloAsync(
        int destinoId, int requisitoId, string tipo, CancellationToken cancellationToken)
    {
        // Grafo: requisito → destino  (arista = "requisito debe hacerse antes que destino")
        // Nuevo arco a agregar: requisitoId → destinoId
        // Hay ciclo si ya existe un camino destinoId → ... → requisitoId en el grafo actual.
        var todas = await correlativiadadRepository.ObtenerTodasPorTipoAsync(tipo, cancellationToken);

        // Adyacencia: desde cada MateriaRequisitoId, a qué MateriaDestinoId apunta
        var grafo = todas
            .GroupBy(c => c.MateriaRequisitoId)
            .ToDictionary(g => g.Key, g => g.Select(c => c.MateriaDestinoId).ToList());

        if (ExisteCamino(grafo, destinoId, requisitoId, new HashSet<int>()))
            throw new BusinessException(
                $"La correlatividad generaría una dependencia circular entre las materias {destinoId} y {requisitoId}.");
    }

    private static bool ExisteCamino(
        Dictionary<int, List<int>> grafo, int actual, int objetivo, HashSet<int> visitados)
    {
        if (!grafo.TryGetValue(actual, out var vecinos)) return false;
        foreach (var v in vecinos)
        {
            if (v == objetivo) return true;
            if (visitados.Add(v) && ExisteCamino(grafo, v, objetivo, visitados)) return true;
        }
        return false;
    }
}
