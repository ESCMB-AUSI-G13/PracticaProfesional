import { Component, OnInit, OnDestroy, signal, computed, ViewChild, ElementRef, Injector, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ReportesService, ReportePromediosCatedra } from '../reportes.service';
import { CursosService, Curso } from '../../cursos/cursos.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-panel-catedras',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-catedras.component.html',
  styleUrl: './panel-catedras.component.scss'
})
export class PanelCatedrasComponent implements OnInit, OnDestroy {
  @ViewChild('chartCanvas') canvasRef?: ElementRef<HTMLCanvasElement>;
  private chart: Chart | null = null;
  cursos  = signal<Curso[]>([]);
  anio    = signal<number | null>(null);
  cursoId = signal<number | null>(null);

  reporte     = signal<ReportePromediosCatedra | null>(null);
  cargando    = signal(false);
  error       = signal<string | null>(null);
  buscado     = signal(false);
  descargando = signal(false);

  promedioGlobal = computed(() => {
    const catedras = this.reporte()?.catedras ?? [];
    const conNota = catedras.filter(c => c.promedioGeneral !== null);
    if (!conNota.length) return null;
    return conNota.reduce((s, c) => s + c.promedioGeneral!, 0) / conNota.length;
  });

  pctAprobacionGlobal = computed(() => {
    const catedras = this.reporte()?.catedras ?? [];
    const total = catedras.reduce((s, c) => s + c.totalConNota, 0);
    const aprobados = catedras.reduce((s, c) => s + c.aprobados, 0);
    return total > 0 ? (aprobados * 100) / total : 0;
  });

  constructor(
    private reportesService: ReportesService,
    private cursosService: CursosService,
    private router: Router,
    private injector: Injector
  ) {}

  ngOnInit(): void {
    this.cursosService.listar().subscribe({ next: c => this.cursos.set(c) });
  }

  buscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.buscado.set(true);

    this.reportesService
      .obtenerPromediosCatedra(this.anio() ?? undefined, this.cursoId() ?? undefined)
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
    this.anio.set(null);
    this.cursoId.set(null);
    this.reporte.set(null);
    this.buscado.set(false);
    this.error.set(null);
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  private colorAprobacion(pct: number): string {
    if (pct >= 70) return 'rgba(39, 174, 96, 0.85)';
    if (pct >= 50) return 'rgba(243, 156, 18, 0.85)';
    return 'rgba(231, 76, 60, 0.85)';
  }

  private initChart(): void {
    if (!this.canvasRef) return;
    this.chart?.destroy();
    const catedras = this.reporte()?.catedras ?? [];
    if (!catedras.length) return;

    this.chart = new Chart(this.canvasRef.nativeElement, {
      type: 'bar',
      data: {
        labels: catedras.map(c => `${c.materiaNombre} (${c.comision})`),
        datasets: [
          {
            label: '% Aprobación',
            data: catedras.map(c => c.porcentajeAprobacion),
            backgroundColor: catedras.map(c => this.colorAprobacion(c.porcentajeAprobacion)),
            borderRadius: 4,
          },
          {
            label: 'Promedio (×10)',
            data: catedras.map(c => (c.promedioGeneral ?? 0) * 10),
            backgroundColor: 'rgba(189, 195, 199, 0.7)',
            borderRadius: 4,
          }
        ]
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        plugins: {
          legend: { position: 'bottom' },
          tooltip: {
            callbacks: {
              label: ctx => {
                if (ctx.datasetIndex === 1) {
                  const real = (ctx.raw as number) / 10;
                  return ` Promedio: ${real.toFixed(2)}`;
                }
                return ` % Aprobación: ${(ctx.raw as number).toFixed(1)}%`;
              }
            }
          }
        },
        scales: { x: { min: 0, max: 100, title: { display: true, text: 'Valor' } } }
      }
    });
  }

  irAlDashboard(): void { this.router.navigate(['/dashboard']); }

  descargarPdf(): void {
    this.descargando.set(true);
    this.reportesService.descargarPromediosCatedraPdf(
      this.anio()    ?? undefined,
      this.cursoId() ?? undefined,
    ).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'promedios-catedra.pdf';
        a.click();
        URL.revokeObjectURL(url);
        this.descargando.set(false);
      },
      error: () => this.descargando.set(false),
    });
  }
}
