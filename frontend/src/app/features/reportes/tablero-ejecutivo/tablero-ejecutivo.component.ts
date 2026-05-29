import {
  Component, OnInit, OnDestroy, ElementRef, ViewChild,
  signal, Injector, afterNextRender
} from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import {
  ReportesService, TableroEjecutivo, EvolucionCohorteResumen
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
  @ViewChild('barrasCanvas')  barrasCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('riesgoCanvas')  riesgoCanvas!: ElementRef<HTMLCanvasElement>;

  private barrasChart: Chart | null = null;
  private riesgoChart: Chart | null = null;

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
          this.renderBarras(data.evolucionCohortes);
          this.renderRiesgo(data);
        }, { injector: this.injector });
      },
      error: () => {
        this.error.set('No se pudo cargar el tablero. Intentá nuevamente.');
        this.cargando.set(false);
      }
    });
  }

  ngOnDestroy(): void {
    this.barrasChart?.destroy();
    this.riesgoChart?.destroy();
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

  private renderBarras(cohortes: EvolucionCohorteResumen[]): void {
    if (!this.barrasCanvas) return;
    this.barrasChart?.destroy();

    this.barrasChart = new Chart(this.barrasCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels: cohortes.map(c => String(c.anioCohorte)),
        datasets: [
          {
            label: 'Activos',
            data: cohortes.map(c => c.activos),
            backgroundColor: '#3498db',
          },
          {
            label: 'Egresados',
            data: cohortes.map(c => c.egresados),
            backgroundColor: '#2ecc71',
          },
          {
            label: 'Desertores',
            data: cohortes.map(c => c.desertores),
            backgroundColor: '#e74c3c',
          },
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'top', labels: { font: { size: 12 }, padding: 12 } },
        },
        scales: {
          x: { stacked: false },
          y: { beginAtZero: true, ticks: { stepSize: 1 } }
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
