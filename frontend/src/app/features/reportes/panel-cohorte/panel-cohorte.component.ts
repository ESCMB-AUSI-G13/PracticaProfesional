import { Component, OnInit, OnDestroy, ElementRef, ViewChild, signal, Injector, effect, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportesService, ReporteRetencionCohorte } from '../reportes.service';
import { CarrerasService, Carrera } from '../../carreras/carreras.service';
import {
  Chart,
  BarController,
  BarElement,
  CategoryScale,
  LinearScale,
  Tooltip,
  Legend,
} from 'chart.js';

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip, Legend);

@Component({
  selector: 'app-panel-cohorte',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-cohorte.component.html',
  styleUrl: './panel-cohorte.component.scss'
})
export class PanelCohorteComponent implements OnInit, OnDestroy {
  @ViewChild('barrasCanvas') barrasCanvas!: ElementRef<HTMLCanvasElement>;
  private barrasChart: Chart | null = null;

  carreras = signal<Carrera[]>([]);

  // Filtros
  carreraId   = signal<number | null>(null);
  anioCohorte = signal<number | null>(null);

  readonly anios: number[] = Array.from(
    { length: new Date().getFullYear() - 2014 },
    (_, i) => new Date().getFullYear() - i
  );

  aniosDisponibles = signal<Set<number>>(new Set());

  // Estado
  reporte  = signal<ReporteRetencionCohorte | null>(null);
  cargando = signal(false);
  error    = signal<string | null>(null);
  buscado  = signal(false);

  constructor(
    private injector: Injector,
    private reportesService: ReportesService,
    private carrerasService: CarrerasService,
  ) {
    effect(() => {
      const carreraId = this.carreraId();
      this.reportesService.obtenerAniosCohorte(carreraId ?? undefined).subscribe({
        next: anios => {
          this.aniosDisponibles.set(new Set(anios));
          const sel = this.anioCohorte();
          if (sel !== null && !anios.includes(sel)) {
            this.anioCohorte.set(null);
          }
        },
        error: () => {
          // Si el endpoint falla, habilitamos todos los años para no bloquear al usuario
          this.aniosDisponibles.set(new Set(this.anios));
        }
      });
    });
  }

  ngOnInit(): void {
    this.carrerasService.listar().subscribe({
      next: data => this.carreras.set(data)
    });
  }

  ngOnDestroy(): void {
    this.barrasChart?.destroy();
  }

  buscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.buscado.set(true);

    this.reportesService.obtenerRetencionCohorte(
      this.carreraId()   ?? undefined,
      this.anioCohorte() ?? undefined,
    ).subscribe({
      next: data => {
        this.reporte.set(data);
        this.cargando.set(false);
        afterNextRender(() => this.renderBarras(), { injector: this.injector });
      },
      error: () => {
        this.error.set('Error al generar el reporte. Intentá nuevamente.');
        this.cargando.set(false);
      }
    });
  }

  private renderBarras(): void {
    const r = this.reporte();
    if (!r || !this.barrasCanvas) return;

    this.barrasChart?.destroy();

    // Agrupar por anioCohorte sumando todas las carreras
    const porAnio = new Map<number, { activos: number; egresados: number; desertores: number }>();
    for (const c of r.cohortes) {
      const prev = porAnio.get(c.anioCohorte) ?? { activos: 0, egresados: 0, desertores: 0 };
      porAnio.set(c.anioCohorte, {
        activos:    prev.activos    + c.activos,
        egresados:  prev.egresados  + c.egresados,
        desertores: prev.desertores + c.desertores,
      });
    }

    const anios = [...porAnio.keys()].sort();

    this.barrasChart = new Chart(this.barrasCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels: anios.map(a => `Cohorte ${a}`),
        datasets: [
          {
            label: 'Activos',
            data: anios.map(a => porAnio.get(a)!.activos),
            backgroundColor: '#3498db',
            borderRadius: 4,
          },
          {
            label: 'Egresados',
            data: anios.map(a => porAnio.get(a)!.egresados),
            backgroundColor: '#2ecc71',
            borderRadius: 4,
          },
          {
            label: 'Desertores',
            data: anios.map(a => porAnio.get(a)!.desertores),
            backgroundColor: '#e74c3c',
            borderRadius: 4,
          },
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'top', labels: { font: { size: 12 }, padding: 14 } },
          tooltip: {
            callbacks: {
              label: ctx => ` ${ctx.dataset.label}: ${ctx.raw} estudiantes`
            }
          }
        },
        scales: {
          x: { stacked: false, grid: { color: '#f0f0f0' } },
          y: { beginAtZero: true, ticks: { stepSize: 5 }, grid: { color: '#f0f0f0' } }
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
    this.barrasChart?.destroy();
    this.barrasChart = null;
  }
}
