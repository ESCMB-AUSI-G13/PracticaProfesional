import { Component, OnInit, OnDestroy, signal, computed, ViewChild, ElementRef, Injector, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ReportesService, ReporteComparativoComisiones } from '../reportes.service';
import { MateriasService, Materia } from '../../materias/materias.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-panel-comisiones',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-comisiones.component.html',
  styleUrl: './panel-comisiones.component.scss'
})
export class PanelComisionesComponent implements OnInit, OnDestroy {
  @ViewChild('chartCanvas') canvasRef?: ElementRef<HTMLCanvasElement>;
  private chart: Chart | null = null;
  materias  = signal<Materia[]>([]);
  materiaId = signal<number | null>(null);
  anio      = signal<number | null>(null);

  reporte  = signal<ReporteComparativoComisiones | null>(null);
  cargando = signal(false);
  error    = signal<string | null>(null);
  buscado  = signal(false);

  promedioGeneral = computed(() => {
    const filas = this.reporte()?.comisiones ?? [];
    const conNota = filas.filter(f => f.promedioGeneral !== null);
    if (!conNota.length) return null;
    return conNota.reduce((s, f) => s + f.promedioGeneral!, 0) / conNota.length;
  });

  pctAprobacionGlobal = computed(() => {
    const filas = this.reporte()?.comisiones ?? [];
    const total = filas.reduce((s, f) => s + f.totalConNota, 0);
    const aprobados = filas.reduce((s, f) => s + f.aprobados, 0);
    return total > 0 ? (aprobados * 100) / total : 0;
  });

  constructor(
    private reportesService: ReportesService,
    private materiasService: MateriasService,
    private router: Router,
    private injector: Injector
  ) {}

  ngOnInit(): void {
    this.materiasService.listar().subscribe({ next: m => this.materias.set(m) });
  }

  buscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.buscado.set(true);

    this.reportesService
      .obtenerComparativoComisiones(this.materiaId() ?? undefined, this.anio() ?? undefined)
      .subscribe({
        next: data => {
          this.reporte.set(data);
          this.cargando.set(false);
          afterNextRender(() => this.initChart(), { injector: this.injector });
        },
        error: () => { this.error.set('Error al generar el reporte.'); this.cargando.set(false); }
      });
  }

  limpiar(): void {
    this.chart?.destroy();
    this.chart = null;
    this.materiaId.set(null);
    this.anio.set(null);
    this.reporte.set(null);
    this.buscado.set(false);
    this.error.set(null);
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  private initChart(): void {
    if (!this.canvasRef) return;
    this.chart?.destroy();
    const filas = this.reporte()?.comisiones ?? [];
    if (!filas.length) return;

    this.chart = new Chart(this.canvasRef.nativeElement, {
      type: 'bar',
      data: {
        labels: filas.map(f => f.comision),
        datasets: [
          {
            label: '% Aprobación',
            data: filas.map(f => f.porcentajeAprobacion),
            backgroundColor: 'rgba(52, 152, 219, 0.85)',
          },
          {
            label: 'Promedio',
            data: filas.map(f => f.promedioGeneral ?? 0),
            backgroundColor: 'rgba(189, 195, 199, 0.85)',
          }
        ]
      },
      options: {
        responsive: true,
        plugins: { legend: { position: 'bottom' } },
        scales: { y: { min: 0, max: 100, title: { display: true, text: 'Valor' } } }
      }
    });
  }

  irAlDashboard(): void { this.router.navigate(['/dashboard']); }
}
