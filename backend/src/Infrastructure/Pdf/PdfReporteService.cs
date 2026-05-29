using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PracticaProfesional.Application.Reportes.DTOs;

namespace PracticaProfesional.Infrastructure.Pdf;

public class PdfReporteService
{
    private const string Institucion    = "Instituto Superior del Profesorado";
    private const string InstitucionSub = "en Ciencias Económicas y Jurídicas \"Dr. José A. Ortiz y Herrera\"";

    // ── Helpers comunes ──────────────────────────────────────────────────────────

    private static IDocument BuildDoc(string titulo, Action<ColumnDescriptor> body)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.8f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(inner =>
                        {
                            inner.Item().Text(Institucion).Bold().FontSize(12).FontColor("#1a2f5a");
                            inner.Item().Text(InstitucionSub).FontSize(8).FontColor("#666666");
                        });
                        row.AutoItem().AlignRight().Column(inner =>
                        {
                            inner.Item().Text(titulo).Bold().FontSize(11).FontColor("#1a2f5a").AlignRight();
                            inner.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7.5f).FontColor("#999999").AlignRight();
                        });
                    });
                    col.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor("#2471a3");
                });

                page.Content().PaddingTop(10).Column(body);

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ").FontSize(8).FontColor("#999999");
                    x.CurrentPageNumber().FontSize(8).FontColor("#999999");
                    x.Span(" de ").FontSize(8).FontColor("#999999");
                    x.TotalPages().FontSize(8).FontColor("#999999");
                });
            });
        });
    }

    private static void SectionTitle(ColumnDescriptor col, string text)
        => col.Item().PaddingTop(10).PaddingBottom(5)
               .Text(text).Bold().FontSize(10).FontColor("#2c3e50");

    private static void KpiRow(ColumnDescriptor col, params (string Label, string Value, string Color)[] kpis)
    {
        col.Item().PaddingBottom(10).Row(row =>
        {
            foreach (var (label, value, color) in kpis)
            {
                row.RelativeItem().Border(1).BorderColor("#e0e0e0").Padding(6).Column(inner =>
                {
                    inner.Item().Text(value).Bold().FontSize(16).FontColor(color).AlignCenter();
                    inner.Item().Text(label).FontSize(7.5f).FontColor("#666666").AlignCenter();
                });
            }
        });
    }

    private static IContainer HeaderCell(IContainer c)
        => c.Background("#eaf4fb").Padding(5);

    private static IContainer DataCell(IContainer c)
        => c.BorderBottom(1).BorderColor("#f0f0f0").Padding(4);

    // ── Tablero Ejecutivo ────────────────────────────────────────────────────────

    public byte[] GenerarTableroEjecutivo(TableroEjecutivoDto dto)
    {
        return BuildDoc("Tablero Ejecutivo", col =>
        {
            SectionTitle(col, "Matrícula Histórica");
            KpiRow(col,
                ("Activos hoy",     dto.TotalMatriculados.ToString(), "#2471a3"),
                ("Egresados",       dto.TotalEgresados.ToString(),    "#1e8449"),
                ("Desertores",      dto.TotalDesertores.ToString(),   "#c0392b"),
                ("Total histórico", dto.TotalHistorico.ToString(),    "#555555"));

            SectionTitle(col, "Tasas Institucionales");
            col.Item().PaddingBottom(10).Table(table =>
            {
                table.ColumnsDefinition(c => { for (int i = 0; i < 4; i++) c.RelativeColumn(); });
                table.Header(h =>
                {
                    foreach (var label in new[] { "Retención", "Egreso", "Deserción", "Aprobación" })
                        h.Cell().Element(HeaderCell).Text(label).Bold().FontSize(8);
                });
                var tasas = new[]
                {
                    ($"{dto.TasaRetencionGlobal:F1}%",         "#2471a3"),
                    ($"{dto.TasaEgresoGlobal:F1}%",            "#1e8449"),
                    ($"{dto.TasaDesercionGlobal:F1}%",         "#c0392b"),
                    ($"{dto.PorcentajeAprobacionGlobal:F1}%",  "#d68910"),
                };
                foreach (var (value, color) in tasas)
                    table.Cell().Element(DataCell).Text(value).Bold().FontSize(12).FontColor(color).AlignCenter();
            });

            if (dto.PromedioNotaGlobal.HasValue)
                col.Item().PaddingBottom(6)
                   .Text($"Promedio institucional de notas: {dto.PromedioNotaGlobal:F2} / 10")
                   .FontSize(9).FontColor("#444444");

            SectionTitle(col, "Riesgo Académico (Estudiantes Activos)");
            KpiRow(col,
                ("Riesgo Alto",      dto.RiesgoAlto.ToString(),              "#c0392b"),
                ("Riesgo Medio",     dto.RiesgoMedio.ToString(),             "#d68910"),
                ("Riesgo Bajo",      dto.RiesgoBajo.ToString(),              "#1e8449"),
                ("% en riesgo alto", $"{dto.PorcentajeRiesgoAlto:F1}%",     "#6c3483"));

            SectionTitle(col, "Evolución por Cohorte");
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(55);
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                });
                table.Header(h =>
                {
                    foreach (var label in new[] { "Cohorte", "Total", "Activos", "Egresados", "Desertores", "% Ret.", "% Des." })
                        h.Cell().Element(HeaderCell).Text(label).Bold().FontSize(8);
                });
                foreach (var c in dto.EvolucionCohortes)
                {
                    var tasaRet = c.Total > 0 ? (c.Activos + c.Egresados) * 100.0 / c.Total : 0;
                    var tasaDes = c.Total > 0 ? c.Desertores * 100.0 / c.Total : 0;
                    table.Cell().Element(DataCell).Text(c.AnioCohorte.ToString()).Bold().FontSize(8);
                    table.Cell().Element(DataCell).Text(c.Total.ToString()).FontSize(8);
                    table.Cell().Element(DataCell).Text(c.Activos.ToString()).FontColor("#2471a3").FontSize(8);
                    table.Cell().Element(DataCell).Text(c.Egresados.ToString()).FontColor("#1e8449").FontSize(8);
                    table.Cell().Element(DataCell).Text(c.Desertores.ToString()).FontColor("#c0392b").FontSize(8);
                    table.Cell().Element(DataCell).Text($"{tasaRet:F1}%").FontSize(8);
                    table.Cell().Element(DataCell).Text($"{tasaDes:F1}%").FontSize(8);
                }
            });
        }).GeneratePdf();
    }

    // ── Riesgo Académico ─────────────────────────────────────────────────────────

    public byte[] GenerarRiesgoAcademico(ReporteRiesgoAcademicoDto dto)
    {
        return BuildDoc("Riesgo Académico", col =>
        {
            SectionTitle(col, "Resumen");
            KpiRow(col,
                ("Riesgo Alto",  dto.TotalAlto.ToString(),          "#c0392b"),
                ("Riesgo Medio", dto.TotalMedio.ToString(),         "#d68910"),
                ("Riesgo Bajo",  dto.TotalBajo.ToString(),          "#1e8449"),
                ("Total",        dto.Estudiantes.Count.ToString(),  "#2471a3"));

            SectionTitle(col, $"Detalle por Estudiante ({dto.Estudiantes.Count})");
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(48);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.ConstantColumn(30);
                    c.ConstantColumn(45);
                    c.RelativeColumn();
                    c.ConstantColumn(42);
                    c.ConstantColumn(42);
                    c.ConstantColumn(38);
                });
                table.Header(h =>
                {
                    foreach (var label in new[] { "Legajo", "Nombre", "Carrera", "Año", "Cohorte", "Condición", "Riesgo", "% Inasist.", "Promedio" })
                        h.Cell().Element(HeaderCell).Text(label).Bold().FontSize(7.5f);
                });
                foreach (var e in dto.Estudiantes)
                {
                    var riesgoColor = e.NivelRiesgo switch
                    {
                        "Alto"  => "#c0392b",
                        "Medio" => "#d68910",
                        _       => "#1e8449"
                    };
                    table.Cell().Element(DataCell).Text(e.Legajo).FontSize(8);
                    table.Cell().Element(DataCell).Text(e.NombreCompleto).FontSize(8);
                    table.Cell().Element(DataCell).Text(e.Carrera).FontSize(7.5f);
                    table.Cell().Element(DataCell).Text(e.AnioCarrera + "°").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(e.AnioCohorte.ToString()).FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(e.Condicion).FontSize(8);
                    table.Cell().Element(DataCell).Text(e.NivelRiesgo).Bold().FontSize(8).FontColor(riesgoColor).AlignCenter();
                    table.Cell().Element(DataCell).Text($"{e.PorcentajeInasistencias:F1}%").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(e.PromedioNotas.HasValue ? $"{e.PromedioNotas:F1}" : "—").FontSize(8).AlignCenter();
                }
            });
        }).GeneratePdf();
    }

    // ── Retención por Cohorte ────────────────────────────────────────────────────

    public byte[] GenerarRetencionCohorte(ReporteRetencionCohorteDto dto)
    {
        return BuildDoc("Retención por Cohorte", col =>
        {
            SectionTitle(col, "Resumen Global");
            KpiRow(col,
                ("Retención global",  $"{dto.TasaRetencionGlobal:F1}%", "#2471a3"),
                ("Deserción global",  $"{dto.TasaDesercionGlobal:F1}%", "#c0392b"),
                ("Total estudiantes", dto.TotalGeneral.ToString(),       "#555555"));

            SectionTitle(col, "Detalle por Cohorte");
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(52);
                    c.RelativeColumn(2);
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                });
                table.Header(h =>
                {
                    foreach (var label in new[] { "Cohorte", "Carrera", "Total", "Activos", "Egresados", "Desertores", "% Ret.", "% Des.", "% Egr." })
                        h.Cell().Element(HeaderCell).Text(label).Bold().FontSize(7.5f);
                });
                foreach (var c in dto.Cohortes)
                {
                    table.Cell().Element(DataCell).Text(c.AnioCohorte.ToString()).Bold().FontSize(8);
                    table.Cell().Element(DataCell).Text(c.Carrera).FontSize(7.5f);
                    table.Cell().Element(DataCell).Text(c.Total.ToString()).FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(c.Activos.ToString()).FontColor("#2471a3").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(c.Egresados.ToString()).FontColor("#1e8449").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(c.Desertores.ToString()).FontColor("#c0392b").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text($"{c.TasaRetencion:F1}%").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text($"{c.TasaDesercion:F1}%").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text($"{c.TasaEgreso:F1}%").FontSize(8).AlignCenter();
                }
            });
        }).GeneratePdf();
    }

    // ── Inasistencias (RR-08) ────────────────────────────────────────────────────

    public byte[] GenerarInasistencias(ReporteInasistenciasDto dto)
    {
        return BuildDoc("Reporte de Inasistencias (RR-08)", col =>
        {
            SectionTitle(col, "Resumen");
            KpiRow(col,
                ("Total registros",       dto.TotalRegistros.ToString(),            "#555555"),
                ("Ausencias",             dto.TotalAusentes.ToString(),             "#c0392b"),
                ("Justificadas",          dto.TotalAusentesJustificados.ToString(), "#d68910"),
                ("Presentes",             dto.TotalPresentes.ToString(),            "#1e8449"));

            var registros = dto.Registros.ToList();
            SectionTitle(col, $"Registros ({registros.Count})");
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(48);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn();
                    c.ConstantColumn(55);
                    c.ConstantColumn(70);
                });
                table.Header(h =>
                {
                    foreach (var label in new[] { "Legajo", "Nombre", "Materia", "Curso", "Fecha", "Tipo" })
                        h.Cell().Element(HeaderCell).Text(label).Bold().FontSize(7.5f);
                });
                foreach (var r in registros)
                {
                    var tipoLabel = r.TipoAsistencia == "AusenteJustificado" ? "Justificada" : "Ausente";
                    var tipoColor = r.TipoAsistencia == "AusenteJustificado" ? "#d68910"      : "#c0392b";
                    table.Cell().Element(DataCell).Text(r.Legajo).FontSize(7.5f);
                    table.Cell().Element(DataCell).Text(r.NombreCompleto).FontSize(7.5f);
                    table.Cell().Element(DataCell).Text(r.Materia).FontSize(7f);
                    table.Cell().Element(DataCell).Text(r.Curso).FontSize(7f);
                    table.Cell().Element(DataCell).Text(r.Fecha.ToString("dd/MM/yyyy")).FontSize(7.5f).AlignCenter();
                    table.Cell().Element(DataCell).Text(tipoLabel).FontColor(tipoColor).FontSize(7.5f);
                }
            });
        }).GeneratePdf();
    }

    // ── Promedios por Cátedra (RR-07) ────────────────────────────────────────────

    public byte[] GenerarPromediosCatedra(ReportePromediosCatedraDto dto)
    {
        return BuildDoc("Promedios por Cátedra (RR-07)", col =>
        {
            if (dto.AnioFiltro.HasValue)
                col.Item().PaddingBottom(6).Text($"Año académico: {dto.AnioFiltro}").FontSize(9).FontColor("#555555");

            var catedras = dto.Catedras.ToList();
            SectionTitle(col, $"Cátedras ({catedras.Count})");
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.ConstantColumn(38);
                    c.ConstantColumn(33);
                    c.ConstantColumn(42);
                    c.ConstantColumn(38);
                    c.ConstantColumn(42);
                    c.ConstantColumn(46);
                    c.ConstantColumn(50);
                });
                table.Header(h =>
                {
                    foreach (var label in new[] { "Materia", "Docente", "Com.", "Año", "Estud.", "C/nota", "Aprobados", "Promedio", "% Aprobac." })
                        h.Cell().Element(HeaderCell).Text(label).Bold().FontSize(7.5f);
                });
                foreach (var c in catedras)
                {
                    table.Cell().Element(DataCell).Text(c.MateriaNombre).FontSize(7.5f);
                    table.Cell().Element(DataCell).Text(c.DocenteNombreCompleto).FontSize(7f);
                    table.Cell().Element(DataCell).Text(c.Comision).FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(c.CursoAnio.ToString()).FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(c.TotalEstudiantes.ToString()).FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(c.TotalConNota.ToString()).FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(c.Aprobados.ToString()).FontColor("#1e8449").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(c.PromedioGeneral.HasValue ? $"{c.PromedioGeneral:F2}" : "—").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text($"{c.PorcentajeAprobacion:F1}%").FontSize(8).AlignCenter();
                }
            });
        }).GeneratePdf();
    }

    // ── Control Individual por Legajo (RR-09) ────────────────────────────────────

    public byte[] GenerarControlLegajo(ControlLegajoDto dto)
    {
        var titulo = $"Control Individual — {dto.NombreCompleto}";

        return BuildDoc(titulo, col =>
        {
            // Perfil del estudiante
            col.Item().PaddingBottom(10).Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                table.Header(h =>
                {
                    foreach (var label in new[] { "Legajo", "Condición", "Año de carrera", "Fecha de ingreso" })
                        h.Cell().Element(HeaderCell).Text(label).Bold().FontSize(8);
                });
                table.Cell().Element(DataCell).Text(dto.Legajo).Bold().FontSize(9);
                table.Cell().Element(DataCell).Text(dto.CondicionAcademica).FontSize(9);
                table.Cell().Element(DataCell).Text($"{dto.Anio}°").FontSize(9).AlignCenter();
                table.Cell().Element(DataCell).Text(dto.FechaDeIngreso.ToString("dd/MM/yyyy")).FontSize(9).AlignCenter();
            });

            SectionTitle(col, "Resumen Global");
            KpiRow(col,
                ("Presencia global",    $"{dto.PorcentajePresenciaGlobal:F1}%",      "#2471a3"),
                ("En riesgo",           dto.MateriasEnRiesgo.ToString(),              "#d68910"),
                ("Regularidad perdida", dto.MateriasConRegularidadPerdida.ToString(), "#c0392b"),
                ("Total clases",        dto.TotalClasesGlobal.ToString(),             "#555555"));

            col.Item().PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor("#e0e0e0").Padding(6).Column(inner =>
                {
                    inner.Item().Text(dto.TotalPresentesGlobal.ToString()).Bold().FontSize(14).FontColor("#1e8449").AlignCenter();
                    inner.Item().Text("Presentes").FontSize(7.5f).FontColor("#666666").AlignCenter();
                });
                row.RelativeItem().Border(1).BorderColor("#e0e0e0").Padding(6).Column(inner =>
                {
                    inner.Item().Text(dto.TotalAusentesJustificadosGlobal.ToString()).Bold().FontSize(14).FontColor("#d68910").AlignCenter();
                    inner.Item().Text("Justificadas").FontSize(7.5f).FontColor("#666666").AlignCenter();
                });
                row.RelativeItem().Border(1).BorderColor("#e0e0e0").Padding(6).Column(inner =>
                {
                    inner.Item().Text(dto.TotalAusentesInjustificadosGlobal.ToString()).Bold().FontSize(14).FontColor("#c0392b").AlignCenter();
                    inner.Item().Text("Injustificadas").FontSize(7.5f).FontColor("#666666").AlignCenter();
                });
            });

            var materias = dto.AsistenciasPorMateria.ToList();
            SectionTitle(col, $"Detalle por Materia ({materias.Count})");
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2);
                    c.RelativeColumn();
                    c.ConstantColumn(42);
                    c.ConstantColumn(42);
                    c.ConstantColumn(48);
                    c.ConstantColumn(48);
                    c.ConstantColumn(52);
                    c.ConstantColumn(70);
                });
                table.Header(h =>
                {
                    foreach (var label in new[] { "Materia", "Curso", "Clases", "Presentes", "Just.", "Injust.", "% Presencia", "Estado" })
                        h.Cell().Element(HeaderCell).Text(label).Bold().FontSize(7.5f);
                });
                foreach (var m in materias)
                {
                    var (estadoLabel, estadoColor) = m.PerdioRegularidad
                        ? ("Reg. perdida", "#c0392b")
                        : m.EnRiesgoRegularidad
                            ? ("En riesgo", "#d68910")
                            : ("Regular", "#1e8449");

                    table.Cell().Element(DataCell).Text(m.Materia).FontSize(8);
                    table.Cell().Element(DataCell).Text(m.Curso).FontSize(7.5f);
                    table.Cell().Element(DataCell).Text(m.TotalClases.ToString()).FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(m.Presentes.ToString()).FontColor("#1e8449").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(m.AusentesJustificados.ToString()).FontColor("#d68910").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(m.AusentesInjustificados.ToString()).FontColor("#c0392b").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text($"{m.PorcentajePresencia:F1}%").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(estadoLabel).Bold().FontSize(8).FontColor(estadoColor);
                }
            });
        }).GeneratePdf();
    }

    // ── Evolución de Notas (RR-06) ───────────────────────────────────────────────

    public byte[] GenerarEvolucionNotas(ReporteEvolucionNotasDto dto)
    {
        var subtitulo = string.IsNullOrEmpty(dto.MateriaNombre)
            ? "Todas las materias"
            : dto.MateriaNombre;

        return BuildDoc($"Evolución de Notas (RR-06)", col =>
        {
            col.Item().PaddingBottom(6).Text(subtitulo).FontSize(9).FontColor("#555555").Italic();
            if (dto.AnioFiltro.HasValue)
                col.Item().PaddingBottom(8).Text($"Año: {dto.AnioFiltro}").FontSize(9).FontColor("#555555");

            var puntos = dto.Evolucion.ToList();
            SectionTitle(col, $"Evolución por Período ({puntos.Count} períodos)");
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.ConstantColumn(55);
                    c.ConstantColumn(50);
                    c.ConstantColumn(60);
                    c.ConstantColumn(50);
                    c.ConstantColumn(60);
                });
                table.Header(h =>
                {
                    foreach (var label in new[] { "Período", "Evaluados", "Aprobados", "Desaprobados", "Promedio", "% Aprobac." })
                        h.Cell().Element(HeaderCell).Text(label).Bold().FontSize(8);
                });
                foreach (var p in puntos)
                {
                    table.Cell().Element(DataCell).Text(p.Periodo).FontSize(8);
                    table.Cell().Element(DataCell).Text(p.TotalEvaluados.ToString()).FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(p.Aprobados.ToString()).FontColor("#1e8449").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(p.Desaprobados.ToString()).FontColor("#c0392b").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(p.PromedioGeneral.HasValue ? $"{p.PromedioGeneral:F2}" : "—").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text($"{p.PorcentajeAprobacion:F1}%").FontSize(8).AlignCenter();
                }
            });
        }).GeneratePdf();
    }

    // ── Retención Longitudinal Anual (RR-12) ─────────────────────────────────────

    public byte[] GenerarRetencionAnual(ReporteRetencionAnualDto dto)
    {
        return BuildDoc("Retención Longitudinal por Año (RR-12)", col =>
        {
            col.Item().PaddingBottom(6)
               .Text($"Umbral de alerta: {dto.UmbralAlerta:F0}% — Columnas disponibles: Año 1 a Año {dto.MaxAnios}")
               .FontSize(9).FontColor("#555555");

            var columnas = Enumerable.Range(1, dto.MaxAnios).ToList();

            SectionTitle(col, $"Tasas de Retención por Cohorte ({dto.Cohortes.Count} cohortes)");
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(52);
                    c.RelativeColumn(2);
                    c.ConstantColumn(42);
                    foreach (var _ in columnas) c.RelativeColumn();
                });
                table.Header(h =>
                {
                    h.Cell().Element(HeaderCell).Text("Cohorte").Bold().FontSize(8);
                    h.Cell().Element(HeaderCell).Text("Carrera").Bold().FontSize(8);
                    h.Cell().Element(HeaderCell).Text("Total").Bold().FontSize(8);
                    foreach (var n in columnas)
                        h.Cell().Element(HeaderCell).Text($"Año {n}").Bold().FontSize(8);
                });
                foreach (var c in dto.Cohortes)
                {
                    table.Cell().Element(DataCell).Text(c.AnioCohorte.ToString()).Bold().FontSize(8);
                    table.Cell().Element(DataCell).Text(c.Carrera).FontSize(7.5f);
                    table.Cell().Element(DataCell).Text(c.TotalInicial.ToString()).FontSize(8).AlignCenter();
                    foreach (var n in columnas)
                    {
                        if (c.TasasPorAnio.TryGetValue(n, out var tasa))
                        {
                            var color = tasa >= dto.UmbralAlerta      ? "#1e8449"
                                      : tasa >= dto.UmbralAlerta - 10 ? "#d68910"
                                      :                                  "#c0392b";
                            table.Cell().Element(DataCell).Text($"{tasa:F1}%").Bold().FontSize(8).FontColor(color).AlignCenter();
                        }
                        else
                        {
                            table.Cell().Element(DataCell).Text("—").FontSize(8).FontColor("#aaaaaa").AlignCenter();
                        }
                    }
                }
            });

            if (dto.PromediosPorAnio.Count > 0)
            {
                SectionTitle(col, "Promedios de Retención por Año");
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c => { foreach (var _ in columnas) c.RelativeColumn(); });
                    table.Header(h =>
                    {
                        foreach (var n in columnas)
                            h.Cell().Element(HeaderCell).Text($"Año {n}").Bold().FontSize(8);
                    });
                    foreach (var n in columnas)
                    {
                        if (dto.PromediosPorAnio.TryGetValue(n, out var prom))
                        {
                            var color = prom >= dto.UmbralAlerta      ? "#1e8449"
                                      : prom >= dto.UmbralAlerta - 10 ? "#d68910"
                                      :                                  "#c0392b";
                            table.Cell().Element(DataCell).Text($"{prom:F1}%").Bold().FontSize(9).FontColor(color).AlignCenter();
                        }
                        else
                        {
                            table.Cell().Element(DataCell).Text("—").FontSize(8).FontColor("#aaaaaa").AlignCenter();
                        }
                    }
                });
            }
        }).GeneratePdf();
    }

    // ── Comparativo de Comisiones (RR-05) ─────────────────────────────────────────

    public byte[] GenerarComparativoComisiones(ReporteComparativoComisionesDto dto)
    {
        var titulo = string.IsNullOrEmpty(dto.MateriaNombre)
            ? "Comparativo de Comisiones (RR-05)"
            : $"Comparativo de Comisiones — {dto.MateriaNombre}";

        return BuildDoc(titulo, col =>
        {
            if (dto.AnioFiltro.HasValue)
                col.Item().PaddingBottom(6).Text($"Año académico: {dto.AnioFiltro}").FontSize(9).FontColor("#555555");

            var comisiones = dto.Comisiones.ToList();
            SectionTitle(col, $"Comisiones ({comisiones.Count})");
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(38);
                    c.ConstantColumn(42);
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                });
                table.Header(h =>
                {
                    foreach (var label in new[] { "Año", "Com.", "Inscriptos", "C/nota", "Aprob.", "Desaprob.", "Promedio", "% Aprob." })
                        h.Cell().Element(HeaderCell).Text(label).Bold().FontSize(7.5f);
                });
                foreach (var c in comisiones)
                {
                    table.Cell().Element(DataCell).Text(c.CursoAnio.ToString()).FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(c.Comision).FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(c.TotalInscriptos.ToString()).FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(c.TotalConNota.ToString()).FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(c.Aprobados.ToString()).FontColor("#1e8449").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(c.Desaprobados.ToString()).FontColor("#c0392b").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text(c.PromedioGeneral.HasValue ? $"{c.PromedioGeneral:F2}" : "—").FontSize(8).AlignCenter();
                    table.Cell().Element(DataCell).Text($"{c.PorcentajeAprobacion:F1}%").FontSize(8).AlignCenter();
                }
            });
        }).GeneratePdf();
    }
}
