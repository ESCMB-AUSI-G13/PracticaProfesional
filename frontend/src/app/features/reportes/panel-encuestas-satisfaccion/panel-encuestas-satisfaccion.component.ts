import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EncuestasService, EncuestaDto, ReporteSatisfaccionDto } from '../../encuestas/encuestas.service';

@Component({
  selector: 'app-panel-encuestas-satisfaccion',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-encuestas-satisfaccion.component.html',
  styleUrl: './panel-encuestas-satisfaccion.component.scss'
})
export class PanelEncuestasSatisfaccionComponent implements OnInit {
  encuestas        = signal<EncuestaDto[]>([]);
  encuestaId       = signal<number | null>(null);
  reporte          = signal<ReporteSatisfaccionDto | null>(null);
  cargandoLista    = signal(true);
  cargando         = signal(false);
  error            = signal<string | null>(null);

  constructor(private service: EncuestasService) {}

  ngOnInit(): void {
    this.service.listar().subscribe({
      next: data => { this.encuestas.set(data); this.cargandoLista.set(false); },
      error: () => { this.error.set('Error al cargar lista de encuestas.'); this.cargandoLista.set(false); }
    });
  }

  generar(): void {
    const id = this.encuestaId();
    if (!id) { this.error.set('Seleccioná una encuesta.'); return; }
    this.error.set(null);
    this.cargando.set(true);
    this.service.obtenerResultados(id).subscribe({
      next: data => { this.reporte.set(data); this.cargando.set(false); },
      error: () => { this.error.set('Error al generar el reporte.'); this.cargando.set(false); }
    });
  }

  limpiar(): void {
    this.reporte.set(null);
    this.encuestaId.set(null);
    this.error.set(null);
  }

  barWidth(promedio: number | null): string {
    if (promedio === null) return '0%';
    return `${(promedio / 5) * 100}%`;
  }

  promedioColor(promedio: number | null): string {
    if (promedio === null) return '#ccc';
    if (promedio >= 4) return '#27ae60';
    if (promedio >= 3) return '#f39c12';
    return '#e74c3c';
  }

  estrellasLabel(promedio: number | null): string {
    if (promedio === null) return '—';
    return promedio.toFixed(2) + ' / 5';
  }
}
