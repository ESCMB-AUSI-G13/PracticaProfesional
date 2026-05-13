import { Component, OnInit, OnDestroy, signal, computed, ViewChild, ElementRef, Injector, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ReportesService, ReporteEvolucionNotas } from '../reportes.service';
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
  @ViewChild('chartCanvas') canvasRef?: ElementRef<HTMLCanvasElement>;
  private chart: Chart | null = null;
  materias     = signal<Materia[]>([]);
  materiaId    = signal<number | null>(null);
  anio         = signal<number | null>(null);
  cuatrimestre = signal<number | null>(null);
  anioCarrera  = signal<number | null>(null);
  tipoExamen   = signal<number | null>(null);

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
      )
      .subscribe({
        next: data => {
          this.reporte.set(data);
          this.cargando.set(false);
          afterNextRender(() => this.initChart(), { injector: this.injector });
        },
        error: () => { this.error.set('Error al generar el reporte.'); this.cargando.set(false); }
      });
  }

  limpiar(): void {
    this.chart?.destroy();
    this.chart = null;
    this.materiaId.set(null);
    this.anio.set(null);
    this.cuatrimestre.set(null);
    this.anioCarrera.set(null);
    this.tipoExamen.set(null);
    this.reporte.set(null);
    this.buscado.set(false);
    this.error.set(null);
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  private initChart(): void {
    if (!this.canvasRef) return;
    this.chart?.destroy();
    const puntos = this.reporte()?.evolucion ?? [];
    if (!puntos.length) return;

    // Recolectar todas las carreras únicas presentes en la respuesta
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

    this.chart = new Chart(this.canvasRef.nativeElement, {
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

  irAlDashboard(): void { this.router.navigate(['/dashboard']); }
}
