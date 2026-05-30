import {
  Component, OnInit, OnDestroy, ElementRef, ViewChild,
  signal, Injector, afterNextRender
} from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import {
  ReportesService, TableroEjecutivo
} from '../reportes.service';
import {
  Chart,
  BarController, BarElement, CategoryScale, LinearScale,
  DoughnutController, ArcElement,
  Tooltip, Legend
} from 'chart.js';

Chart.register(
  BarController, BarElement, CategoryScale, LinearScale,
  DoughnutController, ArcElement,
  Tooltip, Legend
);

@Component({
  selector: 'app-tablero-ejecutivo',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  templateUrl: './tablero-ejecutivo.component.html',
  styleUrl: './tablero-ejecutivo.component.scss'
})
export class TableroEjecutivoComponent implements OnInit, OnDestroy {
  @ViewChild('riesgoCanvas')    riesgoCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('matriculaCanvas') matriculaCanvas!: ElementRef<HTMLCanvasElement>;

  private riesgoChart:    Chart | null = null;
  private matriculaChart: Chart | null = null;

  tablero     = signal<TableroEjecutivo | null>(null);
  cargando    = signal(true);
  error       = signal<string | null>(null);
  descargando = signal(false);

  constructor(
    private injector:        Injector,
    private reportesService: ReportesService,
  ) {}

  ngOnInit(): void {
    this.reportesService.obtenerTableroEjecutivo().subscribe({
      next: data => {
        this.tablero.set(data);
        this.cargando.set(false);
        afterNextRender(() => {
          this.renderRiesgo(data);
          this.renderMatricula(data);
        }, { injector: this.injector });
      },
      error: () => {
        this.error.set('No se pudo cargar el tablero. Intentá nuevamente.');
        this.cargando.set(false);
      }
    });
  }

  ngOnDestroy(): void {
    this.riesgoChart?.destroy();
    this.matriculaChart?.destroy();
  }

  descargarPdf(): void {
    this.descargando.set(true);
    this.reportesService.descargarTableroEjecutivoPdf().subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'tablero-ejecutivo.pdf';
        a.click();
        URL.revokeObjectURL(url);
        this.descargando.set(false);
      },
      error: () => this.descargando.set(false),
    });
  }

  private renderMatricula(t: TableroEjecutivo): void {
    if (!t.evolucionMatricula?.length || !this.matriculaCanvas) return;
    this.matriculaChart?.destroy();

    this.matriculaChart = new Chart(this.matriculaCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels: t.evolucionMatricula.map(p => String(p.anio)),
        datasets: [
          {
            label: 'Total activos',
            data: t.evolucionMatricula.map(p => p.totalActivos),
            backgroundColor: '#3498db',
            borderRadius: 4,
          },
          {
            label: 'Ingresantes',
            data: t.evolucionMatricula.map(p => p.ingresantes),
            backgroundColor: '#2ecc71',
            borderRadius: 4,
          },
          {
            label: 'Continuantes',
            data: t.evolucionMatricula.map(p => p.continuantes),
            backgroundColor: '#9b59b6',
            borderRadius: 4,
          },
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'top', labels: { font: { size: 12 }, padding: 12 } },
          tooltip: {
            callbacks: { label: ctx => ` ${ctx.dataset.label}: ${ctx.raw} estudiantes` }
          }
        },
        scales: {
          x: { grid: { color: '#f0f0f0' } },
          y: { beginAtZero: true, grid: { color: '#f0f0f0' } }
        }
      }
    });
  }

  private renderRiesgo(t: TableroEjecutivo): void {
    if (!this.riesgoCanvas) return;
    this.riesgoChart?.destroy();

    this.riesgoChart = new Chart(this.riesgoCanvas.nativeElement, {
      type: 'doughnut',
      data: {
        labels: ['Riesgo Alto', 'Riesgo Medio', 'Riesgo Bajo'],
        datasets: [{
          data: [t.riesgoAlto, t.riesgoMedio, t.riesgoBajo],
          backgroundColor: ['#e74c3c', '#f39c12', '#2ecc71'],
          borderWidth: 2,
          borderColor: '#fff',
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'bottom', labels: { font: { size: 12 }, padding: 12 } },
          tooltip: {
            callbacks: {
              label: ctx => {
                const total = t.riesgoAlto + t.riesgoMedio + t.riesgoBajo;
                const pct   = total > 0 ? Math.round((ctx.raw as number) / total * 100) : 0;
                return ` ${ctx.label}: ${ctx.raw} (${pct}%)`;
              }
            }
          }
        }
      }
    });
  }
}
