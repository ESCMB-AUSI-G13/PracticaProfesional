import { Component, OnInit, OnDestroy, ElementRef, ViewChild, signal, computed, Injector, effect, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  ReportesService,
  ReporteRetencionCohorte,
  ReporteRetencionAnual,
  CohorteRetencionAnual,
} from '../reportes.service';
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
  reporte      = signal<ReporteRetencionCohorte | null>(null);
  reporteAnual = signal<ReporteRetencionAnual | null>(null);
  cargando    = signal(false);
  error       = signal<string | null>(null);
  buscado     = signal(false);
  descargando      = signal(false);
  descargandoAnual = signal(false);

  // Columnas dinámicas para la tabla de retención anual
  columnas = computed<number[]>(() => {
    const r = this.reporteAnual();
    if (!r) return [];
    return Array.from({ length: r.maxAnios }, (_, i) => i + 1);
  });

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
    this.reporteAnual.set(null); // forzar re-fetch con los filtros actuales

    this.reportesService.obtenerRetencionCohorte(
      this.carreraId()   ?? undefined,
      this.anioCohorte() ?? undefined,
    ).subscribe({
      next: data => {
        this.reporte.set(data);
        this.cargando.set(false);
        afterNextRender(() => this.renderBarras(), { injector: this.injector });
        if (this.tabActiva() === 'anual') {
          this.cambiarTab('anual');
        }
      },
      error: () => {
        this.error.set('Error al generar el reporte. Intentá nuevamente.');
        this.cargando.set(false);
      }
    });
  }

  tabActiva    = signal<'cohorte' | 'anual'>('cohorte');
  cargandoAnual = signal(false);

  cambiarTab(tab: 'cohorte' | 'anual'): void {
    this.tabActiva.set(tab);
    if (tab === 'anual' && !this.reporteAnual()) {
      this.cargandoAnual.set(true);
      this.reportesService.obtenerRetencionAnual(
        this.carreraId()   ?? undefined,
        this.anioCohorte() ?? undefined,
      ).subscribe({
        next:  data => { this.reporteAnual.set(data); this.cargandoAnual.set(false); },
        error: ()   => this.cargandoAnual.set(false)
      });
    }
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
    this.reporteAnual.set(null);
    this.tabActiva.set('cohorte');
    this.cargandoAnual.set(false);
    this.buscado.set(false);
    this.error.set(null);
    this.barrasChart?.destroy();
    this.barrasChart = null;
  }

  getTasaAnual(cohorte: CohorteRetencionAnual, anioOrdinal: number): number | null {
    const val = cohorte.tasasPorAnio[anioOrdinal];
    return val !== undefined ? val : null;
  }

  badgeTasa(tasa: number | null, umbral: number): string {
    if (tasa === null) return 'badge-sin-dato';
    if (tasa >= umbral)      return 'badge-verde';
    if (tasa >= umbral - 10) return 'badge-naranja';
    return 'badge-rojo';
  }

  promedioOrdinal(anioOrdinal: number): number | null {
    const r = this.reporteAnual();
    if (!r) return null;
    const val = r.promediosPorAnio[anioOrdinal];
    return val !== undefined ? val : null;
  }

  descargarPdf(): void {
    this.descargando.set(true);
    this.reportesService.descargarRetencionCohortePdf(
      this.carreraId()   ?? undefined,
      this.anioCohorte() ?? undefined,
    ).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'retencion-cohorte.pdf';
        a.click();
        URL.revokeObjectURL(url);
        this.descargando.set(false);
      },
      error: () => this.descargando.set(false),
    });
  }

  descargarPdfAnual(): void {
    this.descargandoAnual.set(true);
    this.reportesService.descargarRetencionAnualPdf(
      this.carreraId()   ?? undefined,
      this.anioCohorte() ?? undefined,
    ).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'retencion-anual.pdf';
        a.click();
        URL.revokeObjectURL(url);
        this.descargandoAnual.set(false);
      },
      error: () => this.descargandoAnual.set(false),
    });
  }
}
