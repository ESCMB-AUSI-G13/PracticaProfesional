import { Component, OnInit, OnDestroy, signal, computed, ViewChild, ElementRef, Injector, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportesService, ControlLegajo } from '../reportes.service';
import { EstudiantesService, EstudianteBusqueda } from '../../estudiantes/estudiantes.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-control-legajo',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './control-legajo.component.html',
  styleUrl: './control-legajo.component.scss'
})
export class ControlLegajoComponent implements OnInit, OnDestroy {
  @ViewChild('asistenciaCanvas') asistenciaCanvas?: ElementRef<HTMLCanvasElement>;
  private chart: Chart | null = null;

  estudiantes          = signal<EstudianteBusqueda[]>([]);
  cargandoEstudiantes  = signal(true);
  errorEstudiantes     = signal<string | null>(null);
  busquedaTexto        = signal('');
  resultado            = signal<ControlLegajo | null>(null);
  cargando             = signal(false);
  error                = signal<string | null>(null);

  sugerencias = computed(() => {
    const q = this.busquedaTexto().trim().toLowerCase();
    if (q.length < 2) return [];
    return this.estudiantes()
      .filter(e =>
        `${e.apellido} ${e.nombre}`.toLowerCase().includes(q) ||
        `${e.nombre} ${e.apellido}`.toLowerCase().includes(q) ||
        e.legajo.toLowerCase().includes(q)
      )
      .slice(0, 12);
  });

  chartHeight = computed(() => {
    const count = this.resultado()?.asistenciasPorMateria.length ?? 0;
    return Math.max(180, count * 42 + 50);
  });

  constructor(
    private reportesService: ReportesService,
    private estudiantesService: EstudiantesService,
    private injector: Injector
  ) {}

  ngOnInit(): void {
    this.estudiantesService.buscarParaAutocompletar().subscribe({
      next: e => {
        this.estudiantes.set(e);
        this.cargandoEstudiantes.set(false);
      },
      error: () => {
        this.cargandoEstudiantes.set(false);
        this.errorEstudiantes.set('No se pudo cargar la lista de alumnos. Podés buscar ingresando el legajo directamente.');
      }
    });
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  seleccionarEstudiante(e: EstudianteBusqueda): void {
    this.busquedaTexto.set(`${e.apellido}, ${e.nombre}`);
    this.buscarPorLegajo(e.legajo);
  }

  buscarManual(): void {
    const texto = this.busquedaTexto().trim();
    if (!texto) return;
    // Si hay una sola sugerencia, la toma directamente
    const sugs = this.sugerencias();
    if (sugs.length === 1) {
      this.seleccionarEstudiante(sugs[0]);
    } else {
      this.buscarPorLegajo(texto);
    }
  }

  private buscarPorLegajo(legajo: string): void {
    this.cargando.set(true);
    this.error.set(null);
    this.resultado.set(null);

    this.reportesService.obtenerControlPorLegajo(legajo).subscribe({
      next: data => {
        this.resultado.set(data);
        this.cargando.set(false);
        afterNextRender(() => this.initChart(), { injector: this.injector });
      },
      error: err => {
        const msg = err.status === 400 || err.status === 404
          ? `No se encontró ningún estudiante con legajo "${legajo}".`
          : 'Error al obtener el control de asistencia.';
        this.error.set(msg);
        this.cargando.set(false);
      }
    });
  }

  limpiar(): void {
    this.busquedaTexto.set('');
    this.resultado.set(null);
    this.error.set(null);
    this.chart?.destroy();
    this.chart = null;
  }

  private initChart(): void {
    const r = this.resultado();
    if (!r || !this.asistenciaCanvas || r.asistenciasPorMateria.length === 0) return;

    this.chart?.destroy();

    const materias = r.asistenciasPorMateria;
    const colores = materias.map(m =>
      m.perdioRegularidad   ? 'rgba(231, 76, 60, 0.85)'  :
      m.enRiesgoRegularidad ? 'rgba(243, 156, 18, 0.85)' :
                              'rgba(39, 174, 96, 0.85)'
    );

    this.chart = new Chart(this.asistenciaCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels: materias.map(m => m.materia),
        datasets: [{
          label: '% Presencia',
          data: materias.map(m => m.porcentajePresencia),
          backgroundColor: colores,
          borderRadius: 4,
        }]
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: ctx => {
                const m = materias[ctx.dataIndex];
                const estado = m.perdioRegularidad   ? '✗ Regularidad perdida'
                             : m.enRiesgoRegularidad ? '⚠ En riesgo'
                             :                        '✓ Regular';
                return ` ${(ctx.raw as number).toFixed(1)}% — ${estado}`;
              }
            }
          }
        },
        scales: {
          x: {
            min: 0, max: 100,
            ticks: { callback: v => v + '%' },
            grid: { color: '#f0f0f0' }
          },
          y: { ticks: { font: { size: 11 } } }
        }
      }
    });
  }

  condicionLabel(condicion: string): string {
    const map: Record<string, string> = {
      'regular':     '● Regular',
      'libre':       '✗ Libre',
      'promocional': '✓ Promocional',
      'egresado':    '★ Egresado',
      'desertor':    '✗ Desertor',
    };
    return map[condicion.toLowerCase()] ?? condicion;
  }
}
