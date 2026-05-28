import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  ReportesService,
  CohorteRetencionAnual,
  ReporteRetencionAnual,
} from '../reportes.service';
import { CarrerasService, Carrera } from '../../carreras/carreras.service';

@Component({
  selector: 'app-panel-retencion-anual',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-retencion-anual.component.html',
  styleUrl: './panel-retencion-anual.component.scss'
})
export class PanelRetencionAnualComponent implements OnInit {
  carreras    = signal<Carrera[]>([]);
  anios       = signal<number[]>([]);

  carreraId   = signal<number | null>(null);
  anioCohorte = signal<number | null>(null);

  reporte  = signal<ReporteRetencionAnual | null>(null);
  cargando = signal(false);
  error    = signal<string | null>(null);
  buscado  = signal(false);

  // Array de años ordinales [1, 2, ..., maxAnios] para iterar en el template
  columnas = computed<number[]>(() => {
    const r = this.reporte();
    if (!r) return [];
    return Array.from({ length: r.maxAnios }, (_, i) => i + 1);
  });

  constructor(
    private reportesService: ReportesService,
    private carrerasService: CarrerasService,
  ) {}

  ngOnInit(): void {
    this.carrerasService.listar().subscribe({
      next: data => this.carreras.set(data)
    });
    this.reportesService.obtenerAniosCohorte().subscribe({
      next: data => this.anios.set(data),
      error: () => this.anios.set([])
    });
  }

  buscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.buscado.set(true);

    this.reportesService.obtenerRetencionAnual(
      this.carreraId()   ?? undefined,
      this.anioCohorte() ?? undefined,
    ).subscribe({
      next: data => {
        this.reporte.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('Error al generar el reporte. Intentá nuevamente.');
        this.cargando.set(false);
      }
    });
  }

  limpiar(): void {
    this.carreraId.set(null);
    this.anioCohorte.set(null);
    this.reporte.set(null);
    this.buscado.set(false);
    this.error.set(null);
  }

  getTasa(cohorte: CohorteRetencionAnual, anioOrdinal: number): number | null {
    const val = cohorte.tasasPorAnio[anioOrdinal];
    return val !== undefined ? val : null;
  }

  badgeTasa(tasa: number | null, umbral: number): string {
    if (tasa === null) return 'badge-sin-dato';
    if (tasa >= umbral)      return 'badge-verde';
    if (tasa >= umbral - 10) return 'badge-naranja';
    return 'badge-rojo';
  }

  promedioOrdinal(anioOrdinal: number): number | null {
    const r = this.reporte();
    if (!r) return null;
    const val = r.promediosPorAnio[anioOrdinal];
    return val !== undefined ? val : null;
  }
}
