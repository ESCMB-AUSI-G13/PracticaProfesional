import { Component, OnInit, OnDestroy, ElementRef, ViewChild, signal, computed, Injector, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  ReportesService,
  RiesgoAcademico,
  ReporteRiesgoAcademico,
} from '../reportes.service';
import { CarrerasService, Carrera } from '../../carreras/carreras.service';
import {
  Chart,
  DoughnutController,
  ArcElement,
  Tooltip,
  Legend,
} from 'chart.js';

Chart.register(DoughnutController, ArcElement, Tooltip, Legend);

@Component({
  selector: 'app-panel-riesgo',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-riesgo.component.html',
  styleUrl: './panel-riesgo.component.scss'
})
export class PanelRiesgoComponent implements OnInit, OnDestroy {
  @ViewChild('donaCanvas') donaCanvas!: ElementRef<HTMLCanvasElement>;
  private donaChart: Chart | null = null;

  carreras   = signal<Carrera[]>([]);
  cohortes   = signal<number[]>([]);

  // Filtros
  anioCohorte     = signal<number | null>(null);
  carreraId       = signal<number | null>(null);
  nivelFiltro     = signal<string>('');
  condicionFiltro = signal<string>('');

  // Estado
  reporte  = signal<ReporteRiesgoAcademico | null>(null);
  cargando = signal(false);
  error    = signal<string | null>(null);
  buscado  = signal(false);

  // Tabla
  busqueda      = signal('');
  sortColumna   = signal<keyof RiesgoAcademico>('nivelRiesgo');
  sortDir       = signal<'asc' | 'desc'>('asc');
  paginaActual  = signal(1);
  readonly tamPagina = 50;

  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  estudiantesFiltrados = computed(() => {
    const r = this.reporte();
    if (!r) return [];

    const busq = this.busqueda().trim().toLowerCase();
    const cond = this.condicionFiltro();

    let lista = r.estudiantes.filter(e => {
      if (cond && e.condicion !== cond) return false;
      if (busq && !e.nombreCompleto.toLowerCase().includes(busq) && !e.legajo.toLowerCase().includes(busq)) return false;
      return true;
    });

    const col = this.sortColumna();
    const dir = this.sortDir();
    lista.sort((a, b) => {
      const va = a[col] ?? '';
      const vb = b[col] ?? '';
      const cmp = va < vb ? -1 : va > vb ? 1 : 0;
      return dir === 'asc' ? cmp : -cmp;
    });

    return lista;
  });

  totalPaginas = computed(() =>
    Math.max(1, Math.ceil(this.estudiantesFiltrados().length / this.tamPagina))
  );

  estudiantesPaginados = computed(() => {
    const inicio = (this.paginaActual() - 1) * this.tamPagina;
    return this.estudiantesFiltrados().slice(inicio, inicio + this.tamPagina);
  });

  constructor(
    private injector: Injector,
    private reportesService: ReportesService,
    private carrerasService: CarrerasService,
  ) {}

  ngOnInit(): void {
    this.carrerasService.listar().subscribe({
      next: data => {
        this.carreras.set(data);
        // Cohortes fijas según el seed (2023-2026)
        this.cohortes.set([2023, 2024, 2025, 2026]);
      }
    });
  }

  ngOnDestroy(): void {
    this.donaChart?.destroy();
  }

  buscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.buscado.set(true);
    this.paginaActual.set(1);

    this.reportesService.obtenerRiesgoAcademico(
      this.anioCohorte() ?? undefined,
      this.carreraId()   ?? undefined,
      this.nivelFiltro() || undefined,
    ).subscribe({
      next: data => {
        this.reporte.set(data);
        this.cargando.set(false);
        afterNextRender(() => this.renderDona(), { injector: this.injector });
      },
      error: () => {
        this.error.set('Error al generar el reporte. Intentá nuevamente.');
        this.cargando.set(false);
      }
    });
  }

  private renderDona(): void {
    const r = this.reporte();
    if (!r || !this.donaCanvas) return;

    this.donaChart?.destroy();

    this.donaChart = new Chart(this.donaCanvas.nativeElement, {
      type: 'doughnut',
      data: {
        labels: ['Alto', 'Medio', 'Bajo'],
        datasets: [{
          data: [r.totalAlto, r.totalMedio, r.totalBajo],
          backgroundColor: ['#e74c3c', '#f39c12', '#2ecc71'],
          borderWidth: 2,
          borderColor: '#fff',
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'bottom', labels: { font: { size: 12 }, padding: 14 } },
          tooltip: {
            callbacks: {
              label: ctx => {
                const total = r.totalAlto + r.totalMedio + r.totalBajo;
                const pct = total > 0 ? Math.round((ctx.raw as number) / total * 100) : 0;
                return ` ${ctx.label}: ${ctx.raw} estudiantes (${pct}%)`;
              }
            }
          }
        }
      }
    });
  }

  onBusquedaChange(valor: string): void {
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => {
      this.busqueda.set(valor);
      this.paginaActual.set(1);
    }, 300);
  }

  ordenarPor(col: keyof RiesgoAcademico): void {
    if (this.sortColumna() === col) {
      this.sortDir.update(d => d === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortColumna.set(col);
      this.sortDir.set('asc');
    }
    this.paginaActual.set(1);
  }

  irPagina(n: number): void {
    if (n >= 1 && n <= this.totalPaginas()) this.paginaActual.set(n);
  }

  limpiar(): void {
    this.anioCohorte.set(null);
    this.carreraId.set(null);
    this.nivelFiltro.set('');
    this.condicionFiltro.set('');
    this.busqueda.set('');
    this.reporte.set(null);
    this.buscado.set(false);
    this.error.set(null);
    this.paginaActual.set(1);
    this.donaChart?.destroy();
    this.donaChart = null;
  }

  badgeRiesgo(nivel: string): string {
    return nivel === 'Alto' ? 'badge-alto' : nivel === 'Medio' ? 'badge-medio' : 'badge-bajo';
  }

  badgeCondicion(condicion: string): string {
    if (condicion === 'Promocional') return 'badge-promo';
    if (condicion === 'Libre')       return 'badge-libre';
    return 'badge-regular';
  }

  flechaSort(col: keyof RiesgoAcademico): string {
    if (this.sortColumna() !== col) return '';
    return this.sortDir() === 'asc' ? ' ↑' : ' ↓';
  }
}
