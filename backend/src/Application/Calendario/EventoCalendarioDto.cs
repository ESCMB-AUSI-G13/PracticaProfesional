using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Calendario;

public record EventoCalendarioDto(
    int      Id,
    string   NombreEvento,
    string   Comision,
    DateTime FechaInicio,
    DateTime FechaFin,
    string   TipoEvento);

public record CrearEventoCalendarioDto(
    string     NombreEvento,
    string     Comision,
    DateTime   FechaInicio,
    DateTime   FechaFin,
    TipoEvento TipoEvento);

public record ModificarEventoCalendarioDto(
    string     NombreEvento,
    string     Comision,
    DateTime   FechaInicio,
    DateTime   FechaFin,
    TipoEvento TipoEvento);
