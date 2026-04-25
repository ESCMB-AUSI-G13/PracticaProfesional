import { Component, OnInit, OnDestroy, signal, computed, ViewChild, ElementRef, Injector, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ReportesService, ReporteEvolucionNotas } from '../reportes.service';
import { MateriasService, Materia } from '../../materias/materias.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-panel-evolucion',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-evolucion.component.html',
  styleUrl: './panel-evolucion.component.scss'
})
export class PanelEvolucionComponent implements OnInit, OnDestroy {
  @ViewChild('chartCanvas') canvasRef?: ElementRef<HTMLCanvasElement>;
  private chart: Chart | null = null;
  materias  = signal<Materia[]>([]);
  materiaId = signal<number | null>(null);
  anio      = signal<number | null>(null);

  reporte  = signal<ReporteEvolucionNotas | null>(null);
  cargando = signal(false);
  error    = signal<string | null>(null);
  buscado  = signal(false);

  mejorPromedio = computed(() => {
    const puntos = this.reporte()?.evolucion ?? [];
    const conNota = puntos.filter(p => p.promedioGeneral !== null);
    if (!conNota.length) return null;
    return Math.max(...conNota.map(p => p.promedioGeneral!));
  });

  mejorPct = computed(() => {
    const puntos = this.reporte()?.evolucion ?? [];
    if (!puntos.length) return null;
    return Math.max(...puntos.map(p => p.porcentajeAprobacion));
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
      .obtenerEvolucionNotas(this.materiaId() ?? undefined, this.anio() ?? undefined)
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
    const puntos = this.reporte()?.evolucion ?? [];
    if (!puntos.length) return;

    this.chart = new Chart(this.canvasRef.nativeElement, {
      type: 'line',
      data: {
        labels: puntos.map(p => p.periodo),
        datasets: [
          {
            label: '% Aprobación',
            data: puntos.map(p => p.porcentajeAprobacion),
            borderColor: 'rgba(52, 152, 219, 1)',
            backgroundColor: 'rgba(52, 152, 219, 0.15)',
            fill: true,
            tension: 0.3,
            pointRadius: 5,
            pointHoverRadius: 7,
          },
          {
            label: 'Promedio',
            data: puntos.map(p => p.promedioGeneral ?? 0),
            borderColor: 'rgba(149, 165, 166, 1)',
            backgroundColor: 'rgba(149, 165, 166, 0.15)',
            fill: true,
            tension: 0.3,
            pointRadius: 5,
            pointHoverRadius: 7,
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
