import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ReportesService, ReportePromediosCatedra } from '../reportes.service';

@Component({
  selector: 'app-panel-catedras',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-catedras.component.html',
  styleUrl: './panel-catedras.component.scss'
})
export class PanelCatedrasComponent {
  anio    = signal<number | null>(null);
  cursoId = signal<number | null>(null);

  reporte  = signal<ReportePromediosCatedra | null>(null);
  cargando = signal(false);
  error    = signal<string | null>(null);
  buscado  = signal(false);

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

  constructor(private reportesService: ReportesService, private router: Router) {}

  buscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.buscado.set(true);

    this.reportesService
      .obtenerPromediosCatedra(this.anio() ?? undefined, this.cursoId() ?? undefined)
      .subscribe({
        next: data => { this.reporte.set(data); this.cargando.set(false); },
        error: () => { this.error.set('Error al generar el reporte.'); this.cargando.set(false); }
      });
  }

  limpiar(): void {
    this.anio.set(null);
    this.cursoId.set(null);
    this.reporte.set(null);
    this.buscado.set(false);
    this.error.set(null);
  }

  irAlDashboard(): void { this.router.navigate(['/dashboard']); }
}
