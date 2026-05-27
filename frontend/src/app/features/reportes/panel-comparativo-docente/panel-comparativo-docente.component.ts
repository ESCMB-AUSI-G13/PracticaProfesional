import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EncuestasService, ReporteComparativoEncuestasDto, FilaComparativoEncuestaDto } from '../../encuestas/encuestas.service';

@Component({
  selector: 'app-panel-comparativo-docente',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './panel-comparativo-docente.component.html',
  styleUrl: './panel-comparativo-docente.component.scss'
})
export class PanelComparativoDocenteComponent implements OnInit {
  reporte  = signal<ReporteComparativoEncuestasDto | null>(null);
  cargando = signal(true);
  error    = signal<string | null>(null);

  constructor(private service: EncuestasService) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.service.obtenerComparativoDocente().subscribe({
      next: data => { this.reporte.set(data); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar el reporte comparativo.'); this.cargando.set(false); }
    });
  }

  promedioColor(promedio: number | null): string {
    if (promedio === null) return '#ccc';
    if (promedio >= 4) return '#1a6b9e';
    if (promedio >= 3) return '#3498db';
    return '#5dade2';
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
