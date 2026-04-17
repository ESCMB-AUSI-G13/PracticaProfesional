import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  ReportesService,
  FiltroInasistencias,
  ReporteInasistencias
} from '../reportes.service';

@Component({
  selector: 'app-panel-inasistencias',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-inasistencias.component.html',
  styleUrl: './panel-inasistencias.component.scss'
})
export class PanelInasistenciasComponent {
  // ── Filtros ────────────────────────────────────────────────────────────────
  cursoId    = signal<number | null>(null);
  materiaId  = signal<number | null>(null);
  fechaDesde = signal('');
  fechaHasta = signal('');
  soloAusencias = signal(true);

  // ── Estado ─────────────────────────────────────────────────────────────────
  reporte   = signal<ReporteInasistencias | null>(null);
  cargando  = signal(false);
  error     = signal<string | null>(null);
  buscado   = signal(false);

  constructor(
    private reportesService: ReportesService,
    private router: Router
  ) {}

  buscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.buscado.set(true);

    const filtro: FiltroInasistencias = {
      soloAusencias: this.soloAusencias(),
      ...(this.cursoId()   && { cursoId:   this.cursoId()! }),
      ...(this.materiaId() && { materiaId: this.materiaId()! }),
      ...(this.fechaDesde() && { fechaDesde: this.fechaDesde() }),
      ...(this.fechaHasta() && { fechaHasta: this.fechaHasta() })
    };

    this.reportesService.obtenerInasistencias(filtro).subscribe({
      next: data => {
        this.reporte.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('Error al generar el reporte. Verificá los filtros ingresados.');
        this.cargando.set(false);
      }
    });
  }

  limpiar(): void {
    this.cursoId.set(null);
    this.materiaId.set(null);
    this.fechaDesde.set('');
    this.fechaHasta.set('');
    this.soloAusencias.set(true);
    this.reporte.set(null);
    this.buscado.set(false);
    this.error.set(null);
  }

  irAlDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}
