import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ReportesService, ReporteComparativoComisiones } from '../reportes.service';

@Component({
  selector: 'app-panel-comisiones',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-comisiones.component.html',
  styleUrl: './panel-comisiones.component.scss'
})
export class PanelComisionesComponent {
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

  constructor(private reportesService: ReportesService, private router: Router) {}

  buscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.buscado.set(true);

    this.reportesService
      .obtenerComparativoComisiones(this.materiaId() ?? undefined, this.anio() ?? undefined)
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
