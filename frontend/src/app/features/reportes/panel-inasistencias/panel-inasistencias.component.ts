import { Component, OnInit, OnDestroy, ElementRef, ViewChild, signal, computed, Injector, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import {
  ReportesService,
  FiltroInasistencias,
  ReporteInasistencias
} from '../reportes.service';
import { CursosService, Curso } from '../../cursos/cursos.service';
import { MateriasService, Materia } from '../../materias/materias.service';
import {
  Chart,
  DoughnutController,
  ArcElement,
  Tooltip,
  Legend,
  BarController,
  CategoryScale,
  LinearScale,
  BarElement,
} from 'chart.js';

Chart.register(DoughnutController, ArcElement, Tooltip, Legend, BarController, CategoryScale, LinearScale, BarElement);

@Component({
  selector: 'app-panel-inasistencias',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-inasistencias.component.html',
  styleUrl: './panel-inasistencias.component.scss'
})
export class PanelInasistenciasComponent implements OnInit, OnDestroy {
  @ViewChild('donaCanvas')    donaCanvas!:    ElementRef<HTMLCanvasElement>;
  @ViewChild('barrasCanvas')  barrasCanvas!:  ElementRef<HTMLCanvasElement>;
  @ViewChild('materiasCanvas') materiasCanvas?: ElementRef<HTMLCanvasElement>;

  private donaChart:    Chart | null = null;
  private barrasChart:  Chart | null = null;
  private materiasChart: Chart | null = null;

  // ── Opciones para selects ──────────────────────────────────────────────────
  cursos    = signal<Curso[]>([]);
  materias  = signal<Materia[]>([]);

  // ── Filtros ────────────────────────────────────────────────────────────────
  cursoId       = signal<number | null>(null);
  materiaId     = signal<number | null>(null);
  fechaDesde    = signal('');
  fechaHasta    = signal('');
  soloAusencias = signal(true);

  // ── Estado ─────────────────────────────────────────────────────────────────
  reporte  = signal<ReporteInasistencias | null>(null);
  cargando = signal(false);
  error    = signal<string | null>(null);
  buscado  = signal(false);

  chartMateriaHeight = computed(() => {
    const r = this.reporte();
    if (!r) return 180;
    const count = new Set(r.registros.map(reg => reg.materia)).size;
    return Math.max(180, count * 40 + 50);
  });

  constructor(
    private injector: Injector,
    private reportesService: ReportesService,
    private cursosService: CursosService,
    private materiasService: MateriasService,
    private router: Router
  ) {}

  ngOnInit(): void {
    forkJoin({
      cursos:   this.cursosService.listar(),
      materias: this.materiasService.listar()
    }).subscribe({
      next: ({ cursos, materias }) => {
        this.cursos.set(cursos);
        this.materias.set(materias);
      }
    });
  }

  ngOnDestroy(): void {
    this.donaChart?.destroy();
    this.barrasChart?.destroy();
    this.materiasChart?.destroy();
  }

  buscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.buscado.set(true);

    const filtro: FiltroInasistencias = {
      soloAusencias: this.soloAusencias(),
      ...(this.cursoId()    && { cursoId:    this.cursoId()!    }),
      ...(this.materiaId()  && { materiaId:  this.materiaId()!  }),
      ...(this.fechaDesde() && { fechaDesde: this.fechaDesde()  }),
      ...(this.fechaHasta() && { fechaHasta: this.fechaHasta()  }),
    };

    this.reportesService.obtenerInasistencias(filtro).subscribe({
      next: data => {
        this.reporte.set(data);
        this.cargando.set(false);
        afterNextRender(() => this.renderCharts(), { injector: this.injector });
      },
      error: () => {
        this.error.set('Error al generar el reporte. Verificá los filtros ingresados.');
        this.cargando.set(false);
      }
    });
  }

  private renderCharts(): void {
    const r = this.reporte();
    if (!r || r.registros.length === 0) return;

    this.donaChart?.destroy();
    this.barrasChart?.destroy();
    this.materiasChart?.destroy();

    // ── Dona: justificadas vs injustificadas ─────────────────────────────────
    this.donaChart = new Chart(this.donaCanvas.nativeElement, {
      type: 'doughnut',
      data: {
        labels: ['Injustificadas', 'Justificadas'],
        datasets: [{
          data: [r.totalAusentes, r.totalAusentesJustificados],
          backgroundColor: ['#e74c3c', '#f39c12'],
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
                const total = r.totalAusentes + r.totalAusentesJustificados;
                const pct = total > 0 ? Math.round((ctx.raw as number) / total * 100) : 0;
                return ` ${ctx.label}: ${ctx.raw} (${pct}%)`;
              }
            }
          }
        }
      }
    });

    // ── Barras horizontales: top 10 alumnos ─────────────────────────────────
    const conteoAlumnos = new Map<string, number>();
    for (const reg of r.registros) {
      conteoAlumnos.set(reg.nombreCompleto, (conteoAlumnos.get(reg.nombreCompleto) ?? 0) + 1);
    }
    const top = [...conteoAlumnos.entries()].sort((a, b) => b[1] - a[1]).slice(0, 10);

    this.barrasChart = new Chart(this.barrasCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels: top.map(([name]) => name),
        datasets: [{
          label: 'Inasistencias',
          data: top.map(([, count]) => count),
          backgroundColor: '#3498db',
          borderRadius: 4,
        }]
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: { beginAtZero: true, ticks: { stepSize: 1 }, grid: { color: '#f0f0f0' } },
          y: { ticks: { font: { size: 11 } } }
        }
      }
    });

    // ── Barras horizontales: inasistencias por materia ───────────────────────
    if (!this.materiasCanvas) return;

    const conteoMaterias = new Map<string, number>();
    for (const reg of r.registros) {
      conteoMaterias.set(reg.materia, (conteoMaterias.get(reg.materia) ?? 0) + 1);
    }
    const topMaterias = [...conteoMaterias.entries()].sort((a, b) => b[1] - a[1]);

    this.materiasChart = new Chart(this.materiasCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels: topMaterias.map(([name]) => name),
        datasets: [{
          label: 'Inasistencias',
          data: topMaterias.map(([, count]) => count),
          backgroundColor: '#8e44ad',
          borderRadius: 4,
        }]
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: { beginAtZero: true, ticks: { stepSize: 1 }, grid: { color: '#f0f0f0' } },
          y: { ticks: { font: { size: 11 } } }
        }
      }
    });
  }

  limpiar(): void {
    this.cursoId.set(null);
    this.materiaId.set(null);
    this.fechaDesde.set('');
    this.fechaHasta.set('');
    this.soloAusencias.set(true);
    this.reporte.set(null);
    this.buscado.set(false);
    this.error.set(null);
    this.donaChart?.destroy();
    this.barrasChart?.destroy();
    this.materiasChart?.destroy();
    this.donaChart    = null;
    this.barrasChart  = null;
    this.materiasChart = null;
  }

  irAlDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}
