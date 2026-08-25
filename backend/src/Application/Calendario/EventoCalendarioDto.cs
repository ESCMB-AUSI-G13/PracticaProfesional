using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Calendario;

public record EventoCalendarioDto(
    int      Id,
    string   NombreEvento,
    string   Comision,
    DateTime FechaInicio,
    DateTime FechaFin,
    string   TipoEvento,
    int?     MateriaId,
    int?     CursoId);

/// <summary>
/// MateriaId/CursoId son opcionales: sin ellos, el evento es un período global (aplica a toda
/// inscripción de ese TipoEvento). Con ellos, el período solo habilita esa materia/curso
/// puntual — ver CHECKLIST.md, Tier 5 #19.
/// </summary>
public record CrearEventoCalendarioDto(
    string     NombreEvento,
    string     Comision,
    DateTime   FechaInicio,
    DateTime   FechaFin,
    TipoEvento TipoEvento,
    int?       MateriaId = null,
    int?       CursoId = null);

public record ModificarEventoCalendarioDto(
    string     NombreEvento,
    string     Comision,
    DateTime   FechaInicio,
    DateTime   FechaFin,
    TipoEvento TipoEvento);
