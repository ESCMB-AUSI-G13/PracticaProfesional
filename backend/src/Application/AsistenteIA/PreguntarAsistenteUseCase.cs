using System.ComponentModel;
using Microsoft.Extensions.AI;
using PracticaProfesional.Application.AsistenteIA.DTOs;
using PracticaProfesional.Application.Carreras;
using PracticaProfesional.Application.Carreras.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes;
using PracticaProfesional.Application.Reportes.DTOs;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.AsistenteIA;

/// <summary>
/// Asistente de IA para Dirección: responde preguntas en lenguaje natural sobre datos
/// institucionales usando tool-calling sobre los UseCases de Reportes ya existentes.
/// El modelo nunca ejecuta SQL ni accede a datos directamente — sólo elige qué reporte
/// consultar y con qué filtros; el resultado agregado se le devuelve para que redacte
/// la respuesta final.
/// </summary>
public class PreguntarAsistenteUseCase(
    IAsistenteIAService asistenteIA,
    IAuditoriaService auditoria,
    ListarCarrerasUseCase listarCarrerasUseCase,
    TableroEjecutivoUseCase tableroUseCase,
    RiesgoAcademicoUseCase riesgoUseCase,
    RetencionPorCohorteUseCase retencionCohorteUseCase,
    RetencionAnualUseCase retencionAnualUseCase,
    DesercionPorAnioUseCase desercionUseCase,
    EgresadosPorCarreraUseCase egresadosUseCase,
    ComparativoComisionesUseCase comparativoUseCase,
    PromediosCatedraUseCase promediosUseCase,
    EvolucionNotasUseCase evolucionUseCase,
    ReporteInasistenciasUseCase inasistenciasUseCase,
    ResultadosEncuestasUseCase encuestasUseCase)
{
    private const int LongitudMaximaPregunta = 500;

    public async Task<AsistenteRespuestaDto> EjecutarAsync(
        PreguntaAsistenteDto pregunta, int usuarioId, CancellationToken ct = default)
    {
        var texto = (pregunta.Pregunta ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(texto))
            throw new BusinessException("La pregunta no puede estar vacía.");
        if (texto.Length > LongitudMaximaPregunta)
            throw new BusinessException($"La pregunta es demasiado larga (máximo {LongitudMaximaPregunta} caracteres).");

        var carreras = await listarCarrerasUseCase.EjecutarAsync(ct);
        var systemPrompt = ConstruirSystemPrompt(carreras);
        var herramientas = ConstruirHerramientas(ct);

        var respuesta = await asistenteIA.PreguntarAsync(systemPrompt, texto, herramientas, ct);

        await auditoria.RegistrarAsync(
            "AsistenteIA", usuarioId.ToString(), "Pregunta",
            valorNuevo: new { pregunta = texto, herramienta = respuesta.HerramientaUsada, respuesta = Resumir(respuesta.Texto) },
            cancellationToken: ct);

        return new AsistenteRespuestaDto(respuesta.Texto, respuesta.HerramientaUsada, DateTime.UtcNow);
    }

    // ── Catálogo de herramientas — AIFunctionFactory deriva el JSON Schema
    //    directamente de la firma de cada función y los [Description]. ────────────

    private List<AITool> ConstruirHerramientas(CancellationToken ct) =>
    [
        AIFunctionFactory.Create(
            method: async () => await tableroUseCase.EjecutarAsync(ct),
            name: "tablero_ejecutivo",
            description: "Métricas globales institucionales: matrícula, tasas de retención/deserción/egreso, rendimiento general. Usar para preguntas generales sobre el estado de la institución."),

        AIFunctionFactory.Create(
            method: async (
                [Description("Año de ingreso de la cohorte a filtrar. Omitir para todas.")] int? anioCohorte,
                [Description("ID numérico de la carrera a filtrar. Omitir para todas.")] int? carreraId,
                [Description("Nivel de riesgo a filtrar: 'Alto', 'Medio' o 'Bajo'. Omitir para todos.")] string? nivelRiesgo) =>
                ProyectarRiesgo(await riesgoUseCase.EjecutarAsync(
                    new FiltroRiesgoAcademicoDto(anioCohorte, carreraId, nivelRiesgo), ct)),
            name: "riesgo_academico",
            description: "Cantidad de estudiantes en riesgo académico (Bajo/Medio/Alto) según asistencia y rendimiento."),

        AIFunctionFactory.Create(
            method: async (
                [Description("ID de carrera. Omitir para todas.")] int? carreraId,
                [Description("Año de cohorte. Omitir para todos.")] int? anioCohorte) =>
                await retencionCohorteUseCase.EjecutarAsync(new FiltroRetencionCohorteDto(carreraId, anioCohorte), ct),
            name: "retencion_cohorte",
            description: "Tasas de retención, deserción y egreso agrupadas por cohorte (año de ingreso)."),

        AIFunctionFactory.Create(
            method: async (
                [Description("ID de carrera. Omitir para todas.")] int? carreraId,
                [Description("Año de cohorte. Omitir para todos.")] int? anioCohorte) =>
                await retencionAnualUseCase.EjecutarAsync(carreraId, anioCohorte, ct),
            name: "retencion_anual",
            description: "Retención longitudinal por año de cursada: qué porcentaje de cada cohorte sigue activo en año 2, 3, 4, etc."),

        AIFunctionFactory.Create(
            method: async (
                [Description("ID de carrera. Omitir para todas.")] int? carreraId,
                [Description("Año de cohorte. Omitir para todos.")] int? anioCohorte) =>
                await desercionUseCase.EjecutarAsync(carreraId, anioCohorte, ct),
            name: "desercion_por_anio",
            description: "Tasa de deserción agrupada por año de cursada (1°, 2°, 3°, 4°)."),

        AIFunctionFactory.Create(
            method: async (
                [Description("ID de carrera. Omitir para todas.")] int? carreraId,
                [Description("Año de cohorte. Omitir para todos.")] int? anioCohorte) =>
                await egresadosUseCase.EjecutarAsync(carreraId, anioCohorte, ct),
            name: "egresados_por_carrera",
            description: "Cantidad de egresados agrupados por carrera y año de cohorte."),

        AIFunctionFactory.Create(
            method: async (
                [Description("ID de la materia. Omitir si no se conoce el ID exacto.")] int? materiaId,
                [Description("Año lectivo a filtrar. Omitir para todos.")] int? anio) =>
                await comparativoUseCase.EjecutarAsync(new FiltroComparativoComisionesDto(materiaId, anio, null), ct),
            name: "comparativo_comisiones",
            description: "Comparativo de rendimiento (aprobación, promedio) entre comisiones de una materia."),

        AIFunctionFactory.Create(
            method: async (
                [Description("ID de la materia. Omitir si no se conoce el ID exacto.")] int? materiaId,
                [Description("Año lectivo a filtrar. Omitir para todos.")] int? anio,
                [Description("Agrupación temporal: 'mensual', 'cuatrimestral' o 'anual'.")] string? granularidad) =>
                await evolucionUseCase.EjecutarAsync(
                    new FiltroEvolucionNotasDto(materiaId, anio, null, null, null, null, granularidad ?? "mensual"), ct),
            name: "evolucion_notas",
            description: "Evolución de notas y porcentaje de aprobación en el tiempo."),

        AIFunctionFactory.Create(
            method: async (
                [Description("Año lectivo a filtrar. Omitir para todos.")] int? anio,
                [Description("ID del curso. Omitir si no se conoce.")] int? cursoId,
                [Description("ID de carrera. Omitir para todas.")] int? carreraId) =>
                await promediosUseCase.EjecutarAsync(new FiltroPromediosCatedraDto(null, anio, cursoId, carreraId), ct),
            name: "promedios_catedra",
            description: "Promedios de notas y porcentaje de aprobación por cátedra (materia + docente + comisión)."),

        AIFunctionFactory.Create(
            method: async (
                [Description("ID del curso. Omitir si no se conoce.")] int? cursoId,
                [Description("Año lectivo del plan (1 a 4). Omitir para todos.")] int? anioLectivo,
                [Description("ID de materia. Omitir si no se conoce.")] int? materiaId,
                [Description("Fecha de inicio del rango, formato YYYY-MM-DD. Omitir para sin límite inferior.")] string? fechaDesde,
                [Description("Fecha de fin del rango, formato YYYY-MM-DD. Omitir para sin límite superior.")] string? fechaHasta) =>
                ProyectarInasistencias(await inasistenciasUseCase.EjecutarAsync(
                    new FiltroInasistenciasDto
                    {
                        CursoId = cursoId,
                        AnioLectivo = anioLectivo,
                        MateriaId = materiaId,
                        FechaDesde = ParsearFecha(fechaDesde),
                        FechaHasta = ParsearFecha(fechaHasta)
                    }, espaciosPermitidos: null, cancellationToken: ct)),
            name: "inasistencias_resumen",
            description: "Totales agregados de asistencia (presentes, ausentes, ausentes justificados) en un rango de fechas. Nunca incluye datos individuales de alumnos."),

        AIFunctionFactory.Create(
            method: async () => await encuestasUseCase.ObtenerComparativoAsync(ct),
            name: "resultados_encuestas",
            description: "Comparativo de resultados (promedio, cantidad de respuestas) entre todas las encuestas de satisfacción institucional.")
    ];

    // Proyecciones anti-PII: nunca se reenvían listados nominales de alumnos a la API externa,
    // sólo los totales agregados que ya calculó el UseCase correspondiente.
    private static object ProyectarRiesgo(ReporteRiesgoAcademicoDto r) =>
        new { r.TotalAlto, r.TotalMedio, r.TotalBajo };

    private static object ProyectarInasistencias(ReporteInasistenciasDto r) =>
        new { r.GeneradoEn, r.TotalRegistros, r.TotalAusentes, r.TotalAusentesJustificados, r.TotalPresentes };

    private static DateTime? ParsearFecha(string? fecha) =>
        DateTime.TryParse(fecha, out var d) ? d : null;

    // ── Prompt del sistema ──────────────────────────────────────────────────────

    private static string ConstruirSystemPrompt(IEnumerable<CarreraDto> carreras)
    {
        var listaCarreras = string.Join(", ", carreras.Select(c => $"{c.Id}: {c.Nombre}"));
        return $"""
            Sos el asistente de datos institucionales para Dirección del Instituto Superior del
            Profesorado en Ciencias Económicas y Jurídicas "Dr. José A. Ortiz y Herrera". Respondés
            preguntas sobre matrícula, deserción, retención, egreso, rendimiento académico,
            inasistencias y encuestas de satisfacción, usando EXCLUSIVAMENTE las herramientas
            disponibles.

            Reglas estrictas:
            - Nunca inventes cifras. Si una herramienta no te da el dato pedido, decilo explícitamente.
            - Si la pregunta no corresponde a ninguna herramienta disponible, respondé que no podés
              responder esa consulta con los datos disponibles, sin intentar adivinar.
            - Sé conciso: 2 a 4 oraciones, en español, con las cifras relevantes.
            - Respondé en texto plano, sin formato Markdown (nada de **negrita**, listas con
              guiones ni encabezados) — el chat no lo renderiza, se vería con asteriscos sueltos.
            - Fecha actual: {DateTime.UtcNow:yyyy-MM-dd}. Usala para resolver expresiones como
              "este año" o "este mes".
            - Carreras disponibles (ID: Nombre): {listaCarreras}. Usá el ID correcto según el nombre
              que mencione el usuario. Si no podés identificar con certeza a qué materia, curso o
              docente se refiere una pregunta (no tenés su ID exacto), omití ese filtro y respondé
              con los datos agregados disponibles en vez de adivinar un ID.
            """;
    }

    private static string Resumir(string texto) => texto.Length <= 200 ? texto : texto[..200];
}
