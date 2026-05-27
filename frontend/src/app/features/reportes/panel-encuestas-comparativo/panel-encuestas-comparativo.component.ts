import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EncuestasService, ReporteComparativoEncuestasDto, FilaComparativoEncuestaDto } from '../../encuestas/encuestas.service';

@Component({
  selector: 'app-panel-encuestas-comparativo',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './panel-encuestas-comparativo.component.html',
  styleUrl: './panel-encuestas-comparativo.component.scss'
})
export class PanelEncuestasComparativoComponent implements OnInit {
  reporte  = signal<ReporteComparativoEncuestasDto | null>(null);
  cargando = signal(true);
  error    = signal<string | null>(null);

  constructor(private service: EncuestasService) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.service.obtenerComparativo().subscribe({
      next: data => { this.reporte.set(data); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar el reporte comparativo.'); this.cargando.set(false); }
    });
  }

  promedioColor(promedio: number | null): string {
    if (promedio === null) return '#ccc';
    if (promedio >= 4) return '#27ae60';
    if (promedio >= 3) return '#f39c12';
    return '#e74c3c';
  }

  barWidth(promedio: number | null): string {
    if (promedio === null) return '0%';
    return `${(promedio / 5) * 100}%`;
  }

  tipoLabel(tipo: string): string {
    return tipo === 'EvaluacionDocente' ? 'Evaluación Docente' : 'Satisfacción General';
  }

  mejorEncuesta(encuestas: FilaComparativoEncuestaDto[]): FilaComparativoEncuestaDto | null {
    const conPromedio = encuestas.filter(e => e.promedioGeneral !== null);
    if (conPromedio.length === 0) return null;
    return conPromedio.reduce((a, b) => (a.promedioGeneral! > b.promedioGeneral! ? a : b));
  }

  totalRespuestas(encuestas: FilaComparativoEncuestaDto[]): number {
    return encuestas.reduce((acc, e) => acc + e.totalRespuestas, 0);
  }

  promedioGeneral(encuestas: FilaComparativoEncuestaDto[]): number | null {
    const conPromedio = encuestas.filter(e => e.promedioGeneral !== null);
    if (conPromedio.length === 0) return null;
    return conPromedio.reduce((acc, e) => acc + e.promedioGeneral!, 0) / conPromedio.length;
  }
}
