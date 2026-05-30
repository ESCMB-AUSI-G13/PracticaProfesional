import {
  Component, OnInit, OnDestroy, ElementRef, ViewChild,
  signal, Injector, afterNextRender
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  ReportesService,
  ReporteEgresadosPorCarrera,
  FilaEgresadoCarrera,
} from '../reportes.service';
import { CarrerasService, Carrera } from '../../carreras/carreras.service';
import {
  Chart,
  BarController, BarElement,
  CategoryScale, LinearScale,
  Tooltip, Legend,
} from 'chart.js';

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip, Legend);

const PALETA = ['#2ecc71', '#3498db', '#9b59b6', '#e67e22', '#e74c3c', '#1abc9c'];

@Component({
  selector: 'app-panel-egresados',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-egresados.component.html',
  styleUrl: './panel-egresados.component.scss'
})
export class PanelEgresadosComponent implements OnInit, OnDestroy {
  @ViewChild('barrasCanvas') barrasCanvas!: ElementRef<HTMLCanvasElement>;
  private chart: Chart | null = null;

  carreras    = signal<Carrera[]>([]);
  cohortes    = signal<number[]>([]);
  carreraId   = signal<number | null>(null);
  anioCohorte = signal<number | null>(null);

  reporte     = signal<ReporteEgresadosPorCarrera | null>(null);
  cargando    = signal(false);
  error       = signal<string | null>(null);
  buscado     = signal(false);

  constructor(
    private injector:        Injector,
    private reportesService: ReportesService,
    private carrerasService: CarrerasService,
  ) {}

  ngOnInit(): void {
    this.carrerasService.listar().subscribe({ next: data => this.carreras.set(data) });
    this.reportesService.obtenerAniosCohorte().subscribe({
      next: anios => this.cohortes.set(anios),
      error: () => this.cohortes.set([])
    });
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  buscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.buscado.set(true);

    this.reportesService.obtenerEgresadosPorCarrera(
      this.carreraId()   ?? undefined,
      this.anioCohorte() ?? undefined,
    ).subscribe({
      next: data => {
        this.reporte.set(data);
        this.cargando.set(false);
        afterNextRender(() => this.renderChart(), { injector: this.injector });
      },
      error: () => {
        this.error.set('Error al generar el reporte. Intentá nuevamente.');
        this.cargando.set(false);
      }
    });
  }

  /** Gráfico de barras agrupadas: eje X = año de cohorte, serie = carrera */
  private renderChart(): void {
    const r = this.reporte();
    if (!r || !r.filas.length || !this.barrasCanvas) return;

    this.chart?.destroy();

    const anios    = [...new Set(r.filas.map(f => f.anioCohorte))].sort();
    const carreras = [...new Set(r.filas.map(f => f.carrera))];

    const datasets = carreras.map((carrera, i) => ({
      label: carrera,
      data: anios.map(a => {
        const fila = r.filas.find(f => f.carrera === carrera && f.anioCohorte === a);
        return fila?.totalEgresados ?? 0;
      }),
      backgroundColor: PALETA[i % PALETA.length],
      borderRadius: 4,
    }));

    this.chart = new Chart(this.barrasCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels: anios.map(String),
        datasets,
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: carreras.length > 1, position: 'top' },
          tooltip: {
            callbacks: {
              label: ctx => ` ${ctx.dataset.label}: ${ctx.raw} egresados`
            }
          }
        },
        scales: {
          x: { grid: { color: '#f0f0f0' }, title: { display: true, text: 'Año de cohorte' } },
          y: { beginAtZero: true, ticks: { stepSize: 1 }, grid: { color: '#f0f0f0' } }
        }
      }
    });
  }

  limpiar(): void {
    this.carreraId.set(null);
    this.anioCohorte.set(null);
    this.reporte.set(null);
    this.buscado.set(false);
    this.error.set(null);
    this.chart?.destroy();
    this.chart = null;
  }

  trackFila(_: number, f: FilaEgresadoCarrera): string {
    return f.carrera + f.anioCohorte;
  }
}
