namespace PracticaProfesional.Application.Cursos.DTOs;

/// <param name="Anio">Año CALENDARIO del curso (ej. 2026) — no el año de cursada.</param>
/// <param name="AnioLectivo">Año de CURSADA dentro del plan de estudios (1 a 6) — pese al
/// nombre, no es el año calendario. Ver <see cref="PracticaProfesional.Domain.Entities.Curso"/>.</param>
public record CrearCursoDto(
    int    Anio,
    int    AnioLectivo,
    string Comision,
    int    Cupo,
    // Pese al nombre, es el UsuarioId del preceptor (no Preceptor.Id) — así lo resuelve
    // CrearCursoUseCase vía ObtenerPorUsuarioIdAsync. Renombrado para que el campo lo diga
    // directamente en vez de sorprender a quien integre el endpoint (ver CHECKLIST.md, Tier 7 #32).
    int    PreceptorUsuarioId,
    int    CarreraId
);
