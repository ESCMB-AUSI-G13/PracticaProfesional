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
import { EspaciosCurricularesService } from '../../espacios-curriculares/espacios-curriculares.service';
import { AuthService } from '../../auth/services/auth.service';
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
  anioLectivoFiltro = signal<number | null>(null);
  materiaId         = signal<number | null>(null);
  fechaDesde        = signal('');
  fechaHasta        = signal('');
  soloAusencias     = signal(true);
  comisionFiltro    = signal<string>('');

  carreraFiltro = signal<string>('');

  carrerasDisponibles = computed(() =>
    [...new Set(this.materias().map(m => m.carreraNombre))].sort()
  );

  materiasFiltradas = computed(() => {
    const carrera = this.carreraFiltro();
    return carrera
      ? this.materias().filter(m => m.carreraNombre === carrera)
      : this.materias();
  });

  comisionesDisponibles = computed(() =>
    [...new Set(this.cursos().map(c => c.comision))].sort()
  );

  aniosDisponibles = computed(() =>
    [...new Set(this.cursos().map(c => c.anioLectivo))].sort((a, b) => a - b)
  );

  anioFijadoPorMateria = computed(() => {
    const mid = this.materiaId();
    if (!mid) return null;
    return this.materias().find(m => m.id === mid)?.anio ?? null;
  });

  // ── Estado ─────────────────────────────────────────────────────────────────
  reporte  = signal<ReporteInasistencias | null>(null);
  cargando = signal(false);
  error    = signal<string | null>(null);
  buscado  = signal(false);

  // ── Tabla: búsqueda, orden y paginación ───────────────────────────────────
  busquedaNombre = signal('');
  sortColumna    = signal<'nombreCompleto' | 'fecha' | 'materia' | 'curso'>('nombreCompleto');
  sortDireccion  = signal<'asc' | 'desc'>('asc');
  paginaActual   = signal(1);
  readonly tamPagina = 50;

  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  registrosFiltrados = computed(() => {
    const r = this.reporte();
    if (!r) return [];

    const busqueda = this.busquedaNombre().trim().toLowerCase();
    let lista = busqueda
      ? r.registros.filter(reg => reg.nombreCompleto.toLowerCase().includes(busqueda))
      : [...r.registros];

    const col = this.sortColumna();
    const dir = this.sortDireccion();
    lista.sort((a, b) => {
      const va = a[col];
      const vb = b[col];
      const cmp = va < vb ? -1 : va > vb ? 1 : 0;
      return dir === 'asc' ? cmp : -cmp;
    });

    return lista;
  });

  totalPaginas = computed(() =>
    Math.max(1, Math.ceil(this.registrosFiltrados().length / this.tamPagina))
  );

  registrosPaginados = computed(() => {
    const inicio = (this.paginaActual() - 1) * this.tamPagina;
    return this.registrosFiltrados().slice(inicio, inicio + this.tamPagina);
  });

  chartMateriaHeight = computed(() => {
    const r = this.reporte();
    if (!r) return 180;
    const materiaCount = new Set(r.registros.map(reg => reg.materia)).size;
    const comisionCount = new Set(r.registros.map(reg => {
      const parts = reg.curso.trim().split(/\s+/);
      return parts[parts.length - 1];
    })).size;
    return Math.max(180, materiaCount * (comisionCount * 26 + 12) + 60);
  });

  constructor(
    private injector: Injector,
    private reportesService: ReportesService,
    private cursosService: CursosService,
    private materiasService: MateriasService,
    private espaciosService: EspaciosCurricularesService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (this.authService.rol() === 'Docente') {
      this.espaciosService.listarMisEspacios().subscribe({
        next: espacios => {
          const uniqueMaterias: Materia[] = [
            ...new Map(espacios.map(e => [e.materiaId, {
              id: e.materiaId, codigo: e.materiaCodigo, nombre: e.materiaNombre,
              carreraId: e.carreraId, carreraNombre: e.carreraNombre, anio: e.materiaAnio
            }])).values()
          ];
          const uniqueCursos: Curso[] = [
            ...new Map(espacios.map(e => [e.cursoId, {
              id: e.cursoId, anio: e.cursoAnio, anioLectivo: e.cursoAnioLectivo,
              comision: e.cursoComision, cupo: 0, estado: 'Activo',
              preceptorId: 0, preceptorNombre: ''
            }])).values()
          ];
          this.materias.set(uniqueMaterias);
          this.cursos.set(uniqueCursos);
        }
      });
    } else {
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
      ...(this.anioLectivoFiltro() && { anioLectivo: this.anioLectivoFiltro()! }),
      ...(this.materiaId()         && { materiaId:   this.materiaId()!         }),
      ...(this.fechaDesde()        && { fechaDesde:  this.fechaDesde()         }),
      ...(this.fechaHasta()        && { fechaHasta:  this.fechaHasta()         }),
      ...(this.comisionFiltro()    && { comision:    this.comisionFiltro()     }),
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

    // ── Dona: presentes / justificadas / injustificadas ──────────────────────
    const conPresentes = r.totalPresentes > 0;
    const donaLabels = conPresentes
      ? ['Injustificadas', 'Justificadas', 'Presentes']
      : ['Injustificadas', 'Justificadas'];
    const donaData = conPresentes
      ? [r.totalAusentes, r.totalAusentesJustificados, r.totalPresentes]
      : [r.totalAusentes, r.totalAusentesJustificados];
    const donaColors = conPresentes
      ? ['#e74c3c', '#f39c12', '#2ecc71']
      : ['#e74c3c', '#f39c12'];

    this.donaChart = new Chart(this.donaCanvas.nativeElement, {
      type: 'doughnut',
      data: {
        labels: donaLabels,
        datasets: [{
          data: donaData,
          backgroundColor: donaColors,
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
                const total = r.totalRegistros;
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

    // ── Barras agrupadas: inasistencias por materia comparando comisiones ────
    if (!this.materiasCanvas) return;

    const extraerComision = (curso: string): string => {
      const parts = curso.trim().split(/\s+/);
      return parts[parts.length - 1] || curso;
    };

    const comisionesMap = new Map<string, Map<string, number>>(); // materia → comision → count
    const comisionesSet = new Set<string>();

    for (const reg of r.registros) {
      const com = extraerComision(reg.curso);
      comisionesSet.add(com);
      if (!comisionesMap.has(reg.materia)) comisionesMap.set(reg.materia, new Map());
      const inner = comisionesMap.get(reg.materia)!;
      inner.set(com, (inner.get(com) ?? 0) + 1);
    }

    const materiasOrdenadas = [...comisionesMap.keys()].sort((a, b) => {
      const totalA = [...(comisionesMap.get(a)?.values() ?? [])].reduce((s, v) => s + v, 0);
      const totalB = [...(comisionesMap.get(b)?.values() ?? [])].reduce((s, v) => s + v, 0);
      return totalB - totalA;
    });

    const comisiones = [...comisionesSet].sort();
    const coloresComision = ['#8e44ad', '#3498db', '#e67e22', '#2ecc71', '#e74c3c'];

    const datasetsMateria = comisiones.map((com, i) => ({
      label: `Com. ${com}`,
      data: materiasOrdenadas.map(m => comisionesMap.get(m)?.get(com) ?? 0),
      backgroundColor: coloresComision[i % coloresComision.length],
      borderRadius: 4,
    }));

    this.materiasChart = new Chart(this.materiasCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels: materiasOrdenadas,
        datasets: datasetsMateria,
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: true,
            position: 'top',
            labels: { font: { size: 12 }, padding: 10 }
          }
        },
        scales: {
          x: { beginAtZero: true, ticks: { stepSize: 1 }, grid: { color: '#f0f0f0' } },
          y: { ticks: { font: { size: 11 } } }
        }
      }
    });
  }

  onBusquedaChange(valor: string): void {
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => {
      this.busquedaNombre.set(valor);
      this.paginaActual.set(1);
    }, 300);
  }

  ordenarPor(col: 'nombreCompleto' | 'fecha' | 'materia' | 'curso'): void {
    if (this.sortColumna() === col) {
      this.sortDireccion.update(d => d === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortColumna.set(col);
      this.sortDireccion.set('asc');
    }
    this.paginaActual.set(1);
  }

  irPagina(n: number): void {
    const total = this.totalPaginas();
    if (n >= 1 && n <= total) this.paginaActual.set(n);
  }

  onCarreraChange(carrera: string): void {
    this.carreraFiltro.set(carrera);
    const mid = this.materiaId();
    if (mid) {
      const materia = this.materias().find(m => m.id === mid);
      if (materia && carrera && materia.carreraNombre !== carrera) {
        this.materiaId.set(null);
        this.anioLectivoFiltro.set(null);
      }
    }
  }

  onMateriaChange(id: number | null): void {
    this.materiaId.set(id);
    const anio = id ? (this.materias().find(m => m.id === id)?.anio ?? null) : null;
    this.anioLectivoFiltro.set(anio);
  }

  limpiar(): void {
    this.carreraFiltro.set('');
    this.anioLectivoFiltro.set(null);
    this.materiaId.set(null);
    this.fechaDesde.set('');
    this.fechaHasta.set('');
    this.soloAusencias.set(true);
    this.comisionFiltro.set('');
    this.busquedaNombre.set('');
    this.sortColumna.set('nombreCompleto');
    this.sortDireccion.set('asc');
    this.paginaActual.set(1);
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
