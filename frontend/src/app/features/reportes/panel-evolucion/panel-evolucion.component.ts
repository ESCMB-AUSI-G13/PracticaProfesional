import { Component, OnInit, OnDestroy, signal, computed, ViewChild, ElementRef, Injector, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ReportesService, ReporteEvolucionNotas, PuntoEvolucionNota } from '../reportes.service';
import { MateriasService, Materia } from '../../materias/materias.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-panel-evolucion',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-evolucion.component.html',
  styleUrl: './panel-evolucion.component.scss'
})
export class PanelEvolucionComponent implements OnInit, OnDestroy {
  @ViewChild('chartCanvas')    canvasRef?:    ElementRef<HTMLCanvasElement>;
  @ViewChild('histogramCanvas') histogramRef?: ElementRef<HTMLCanvasElement>;

  private chart:     Chart | null = null;
  private histogram: Chart | null = null;

  materias     = signal<Materia[]>([]);
  materiaId    = signal<number | null>(null);
  anio         = signal<number | null>(null);
  cuatrimestre = signal<number | null>(null);
  anioCarrera  = signal<number | null>(null);
  tipoExamen   = signal<number | null>(null);
  granularidad = signal<'mensual' | 'cuatrimestral' | 'anual'>('mensual');

  reporte  = signal<ReporteEvolucionNotas | null>(null);
  cargando = signal(false);
  error    = signal<string | null>(null);
  buscado  = signal(false);

  anioMateriaSeleccionada = computed<number | null>(() =>
    this.materias().find(m => m.id === this.materiaId())?.anio ?? null
  );

  mejorPromedio = computed(() => {
    const puntos = this.reporte()?.evolucion ?? [];
    const conNota = puntos.filter(p => p.promedioGeneral !== null);
    if (!conNota.length) return null;
    return Math.max(...conNota.map(p => p.promedioGeneral!));
  });

  mejorPct = computed(() => {
    const puntos = this.reporte()?.evolucion ?? [];
    if (!puntos.length) return null;
    return Math.max(...puntos.map(p => p.porcentajeAprobacion));
  });

  constructor(
    private reportesService: ReportesService,
    private materiasService: MateriasService,
    private router: Router,
    private injector: Injector
  ) {}

  ngOnInit(): void {
    this.materiasService.listar().subscribe({ next: m => this.materias.set(m) });
  }

  onMateriaChange(id: number | null): void {
    this.materiaId.set(id);
    const anio = this.materias().find(m => m.id === id)?.anio ?? null;
    this.anioCarrera.set(anio);
  }

  buscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.buscado.set(true);

    this.reportesService
      .obtenerEvolucionNotas(
        this.materiaId()    ?? undefined,
        this.anio()         ?? undefined,
        this.cuatrimestre() ?? undefined,
        this.anioCarrera()  ?? undefined,
        this.tipoExamen()   ?? undefined,
        this.granularidad(),
      )
      .subscribe({
        next: data => {
          this.reporte.set(data);
          this.cargando.set(false);
          afterNextRender(() => {
            this.initChart();
            this.initHistogram();
          }, { injector: this.injector });
        },
        error: () => { this.error.set('Error al generar el reporte.'); this.cargando.set(false); }
      });
  }

  limpiar(): void {
    this.chart?.destroy();     this.chart     = null;
    this.histogram?.destroy(); this.histogram = null;
    this.materiaId.set(null);
    this.anio.set(null);
    this.cuatrimestre.set(null);
    this.anioCarrera.set(null);
    this.tipoExamen.set(null);
    this.granularidad.set('mensual');
    this.reporte.set(null);
    this.buscado.set(false);
    this.error.set(null);
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
    this.histogram?.destroy();
  }

  private initChart(): void {
    if (!this.canvasRef) return;
    this.chart?.destroy();
    const puntos = this.reporte()?.evolucion ?? [];
    if (!puntos.length) return;

    if (puntos.length === 1) {
      this.initBarChart(puntos[0]);
    } else {
      this.initLineChart(puntos);
    }
  }

  // Gráfico de barras para un único período: muestra promedio general + por carrera
  private initBarChart(punto: PuntoEvolucionNota): void {
    const carreras = punto.porCarrera ?? [];
    const labels   = ['General', ...carreras.map(c => c.carreraNombre)];
    const values   = [punto.promedioGeneral, ...carreras.map(c => c.promedio)];
    const bgColors = values.map(v =>
      v !== null && v >= 4 ? 'rgba(39, 174, 96, 0.75)' : 'rgba(231, 76, 60, 0.75)'
    );
    const borderColors = values.map(v =>
      v !== null && v >= 4 ? 'rgba(39, 174, 96, 1)' : 'rgba(231, 76, 60, 1)'
    );

    this.chart = new Chart(this.canvasRef!.nativeElement, {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Promedio',
          data: values,
          backgroundColor: bgColors,
          borderColor: borderColors,
          borderWidth: 1,
          borderRadius: 4,
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          title: {
            display: true,
            text: `Período ${punto.periodo} — Promedio por carrera`,
            color: '#555',
            font: { size: 13 },
          },
          tooltip: {
            callbacks: {
              afterLabel: (item) => {
                const idx = item.dataIndex;
                if (idx === 0) {
                  return `Aprobados: ${punto.aprobados}/${punto.totalEvaluados} (${punto.porcentajeAprobacion.toFixed(1)}%)`;
                }
                const c = (punto.porCarrera ?? [])[idx - 1];
                return c ? `Aprobados: ${c.totalEvaluados} eval. (${c.porcentajeAprobacion.toFixed(1)}%)` : '';
              }
            }
          }
        },
        scales: {
          y: {
            min: 0, max: 10,
            title: { display: true, text: 'Promedio (1–10)', color: '#555' },
            ticks: { stepSize: 1, color: '#555' },
            grid: { color: 'rgba(0,0,0,0.06)' },
          },
          x: { grid: { display: false } },
        },
      },
    });
  }

  // Gráfico de líneas para múltiples períodos
  private initLineChart(puntos: PuntoEvolucionNota[]): void {
    const carrerasMap = new Map<number, string>();
    puntos.forEach(p => p.porCarrera?.forEach(c => carrerasMap.set(c.carreraId, c.carreraNombre)));
    const carreras = Array.from(carrerasMap.entries());

    const palette = [
      { border: 'rgba(142, 68, 173, 1)', bg: 'rgba(142, 68, 173, 0.08)' },
      { border: 'rgba(230, 126, 34, 1)', bg: 'rgba(230, 126, 34, 0.08)' },
      { border: 'rgba(22, 160, 133, 1)', bg: 'rgba(22, 160, 133, 0.08)' },
      { border: 'rgba(231, 76, 60, 1)',  bg: 'rgba(231, 76, 60,  0.08)' },
    ];

    const datasets: any[] = [
      {
        label: 'Promedio general',
        data: puntos.map(p => p.promedioGeneral),
        borderColor: 'rgba(44, 62, 80, 1)',
        backgroundColor: 'rgba(44, 62, 80, 0.06)',
        borderWidth: 3,
        borderDash: [7, 4],
        tension: 0.35,
        pointRadius: 5,
        pointHoverRadius: 7,
        fill: false,
        spanGaps: true,
      },
      ...carreras.map(([id, nombre], i) => ({
        label: nombre,
        data: puntos.map(p => p.porCarrera?.find(c => c.carreraId === id)?.promedio ?? null),
        borderColor: palette[i % palette.length].border,
        backgroundColor: palette[i % palette.length].bg,
        borderWidth: 2,
        tension: 0.35,
        pointRadius: 4,
        pointHoverRadius: 6,
        fill: false,
        spanGaps: true,
      })),
    ];

    this.chart = new Chart(this.canvasRef!.nativeElement, {
      type: 'line',
      data: { labels: puntos.map(p => p.periodo), datasets },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        interaction: { mode: 'index', intersect: false },
        plugins: {
          legend: { position: 'bottom' },
          tooltip: {
            callbacks: {
              afterBody: (items: any[]) => {
                const idx = items[0]?.dataIndex;
                if (idx === undefined) return [];
                const p = puntos[idx];
                return [`Aprobados: ${p.aprobados}/${p.totalEvaluados} (${p.porcentajeAprobacion.toFixed(1)}%)`];
              },
            },
          },
        },
        scales: {
          x: {
            grid: { display: false },
            ticks: { maxRotation: 45 },
          },
          y: {
            min: 0,
            max: 10,
            title: { display: true, text: 'Promedio (1–10)', color: '#555' },
            ticks: { stepSize: 1, color: '#555' },
            grid: { color: 'rgba(0,0,0,0.06)' },
          },
        },
      },
    });
  }

  // Histograma: distribución de notas 1-10 (agregada sobre todos los períodos)
  private initHistogram(): void {
    if (!this.histogramRef) return;
    this.histogram?.destroy();
    const puntos = this.reporte()?.evolucion ?? [];
    if (!puntos.length) return;

    const cantidades = new Array(11).fill(0);
    puntos.forEach(p =>
      p.distribucionNotas?.forEach(d => { cantidades[d.nota] += d.cantidad; })
    );

    const labels  = Array.from({ length: 10 }, (_, i) => String(i + 1));
    const data    = Array.from({ length: 10 }, (_, i) => cantidades[i + 1]);
    const bgColors = Array.from({ length: 10 }, (_, i) =>
      i + 1 >= 4 ? 'rgba(39, 174, 96, 0.75)' : 'rgba(231, 76, 60, 0.75)'
    );

    this.histogram = new Chart(this.histogramRef.nativeElement, {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Estudiantes',
          data,
          backgroundColor: bgColors,
          borderRadius: 4,
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          title: {
            display: true,
            text: puntos.length > 1 ? 'Distribución de notas (todos los períodos)' : 'Distribución de notas',
            color: '#555',
            font: { size: 13 },
          },
          tooltip: {
            callbacks: {
              label: (item) => ` ${item.raw} estudiante${(item.raw as number) !== 1 ? 's' : ''}`,
            }
          }
        },
        scales: {
          x: {
            title: { display: true, text: 'Nota', color: '#555' },
            grid: { display: false },
          },
          y: {
            title: { display: true, text: 'Estudiantes', color: '#555' },
            ticks: { stepSize: 1, color: '#555' },
            grid: { color: 'rgba(0,0,0,0.06)' },
            beginAtZero: true,
          },
        },
      },
    });
  }

  irAlDashboard(): void { this.router.navigate(['/dashboard']); }
}
