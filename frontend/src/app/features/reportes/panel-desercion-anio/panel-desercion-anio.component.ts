import {
  Component, OnInit, OnDestroy, ElementRef, ViewChild,
  signal, Injector, afterNextRender
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  ReportesService,
  ReporteDesercionPorAnio,
  DesercionPorAnio,
} from '../reportes.service';
import { CarrerasService, Carrera } from '../../carreras/carreras.service';
import {
  Chart,
  BarController, BarElement,
  CategoryScale, LinearScale,
  Tooltip, Legend,
} from 'chart.js';

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip, Legend);

@Component({
  selector: 'app-panel-desercion-anio',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-desercion-anio.component.html',
  styleUrl: './panel-desercion-anio.component.scss'
})
export class PanelDesercionAnioComponent implements OnInit, OnDestroy {
  @ViewChild('desercionCanvas') desercionCanvas!: ElementRef<HTMLCanvasElement>;
  private chart: Chart | null = null;

  carreras    = signal<Carrera[]>([]);
  cohortes    = signal<number[]>([]);
  carreraId   = signal<number | null>(null);
  anioCohorte = signal<number | null>(null);

  reporte     = signal<ReporteDesercionPorAnio | null>(null);
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

    this.reportesService.obtenerDesercionPorAnio(
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

  private renderChart(): void {
    const r = this.reporte();
    if (!r || !this.desercionCanvas) return;

    this.chart?.destroy();

    const labels = r.filas.map(f => `${f.anioCursada}° Año`);

    this.chart = new Chart(this.desercionCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: 'Desertores',
            data: r.filas.map(f => f.desertores),
            backgroundColor: '#2c3e7a',
            borderRadius: 4,
            yAxisID: 'y',
          },
          {
            label: 'Tasa %',
            data: r.filas.map(f => f.tasaDesercion),
            backgroundColor: '#5b8dee',
            borderRadius: 4,
            yAxisID: 'y2',
          },
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'bottom', labels: { font: { size: 12 }, padding: 14 } },
          tooltip: {
            callbacks: {
              label: ctx => ctx.datasetIndex === 0
                ? ` Desertores: ${ctx.raw}`
                : ` Tasa: ${ctx.raw}%`
            }
          }
        },
        scales: {
          y: {
            beginAtZero: true,
            position: 'left',
            title: { display: true, text: 'Desertores' },
            grid: { color: '#f0f0f0' }
          },
          y2: {
            beginAtZero: true,
            position: 'right',
            title: { display: true, text: 'Tasa %' },
            grid: { drawOnChartArea: false },
            max: 100
          }
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

  badgeRiesgo(nivel: string): string {
    return nivel === 'Alto' ? 'badge-alto' : nivel === 'Medio' ? 'badge-medio' : 'badge-bajo';
  }

  trackFila(_: number, f: DesercionPorAnio): number {
    return f.anioCursada;
  }
}
