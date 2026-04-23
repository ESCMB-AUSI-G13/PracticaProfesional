import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ReportesService, ReporteEvolucionNotas } from '../reportes.service';

@Component({
  selector: 'app-panel-evolucion',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-evolucion.component.html',
  styleUrl: './panel-evolucion.component.scss'
})
export class PanelEvolucionComponent {
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

  constructor(private reportesService: ReportesService, private router: Router) {}

  buscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.buscado.set(true);

    this.reportesService
      .obtenerEvolucionNotas(this.materiaId() ?? undefined, this.anio() ?? undefined)
      .subscribe({
        next: data => { this.reporte.set(data); this.cargando.set(false); },
        error: () => { this.error.set('Error al generar el reporte.'); this.cargando.set(false); }
      });
  }

  limpiar(): void {
    this.materiaId.set(null);
    this.anio.set(null);
    this.reporte.set(null);
    this.buscado.set(false);
    this.error.set(null);
  }

  irAlDashboard(): void { this.router.navigate(['/dashboard']); }
}
