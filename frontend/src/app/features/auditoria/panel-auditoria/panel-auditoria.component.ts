import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuditoriaService, AuditoriaLogDto, PaginadoDto, AuditoriaFiltros } from '../auditoria.service';

@Component({
  selector: 'app-panel-auditoria',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-auditoria.component.html',
  styleUrl: './panel-auditoria.component.scss'
})
export class PanelAuditoriaComponent implements OnInit {
  resultado = signal<PaginadoDto<AuditoriaLogDto> | null>(null);
  cargando  = signal(true);
  error     = signal<string | null>(null);

  readonly tiposEntidad = ['', 'Usuario', 'Docente', 'Preceptor', 'Estudiante'];
  readonly acciones     = ['', 'CREAR', 'MODIFICAR', 'DESACTIVAR', 'REACTIVAR'];

  filtros: AuditoriaFiltros = {
    entidadTipo: '',
    accion: '',
    ejecutorEmail: '',
    fechaDesde: '',
    fechaHasta: '',
    pagina: 1,
    tamanoPagina: 50
  };

  constructor(
    private auditoriaService: AuditoriaService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);

    const filtrosLimpios: AuditoriaFiltros = {
      ...this.filtros,
      entidadTipo: this.filtros.entidadTipo || undefined,
      accion:      this.filtros.accion      || undefined,
      ejecutorEmail: this.filtros.ejecutorEmail || undefined,
      fechaDesde:  this.filtros.fechaDesde  || undefined,
      fechaHasta:  this.filtros.fechaHasta  || undefined
    };

    this.auditoriaService.listarLogs(filtrosLimpios).subscribe({
      next: (data) => {
        this.resultado.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('Error al cargar el registro de auditoría.');
        this.cargando.set(false);
      }
    });
  }

  aplicarFiltros(): void {
    this.filtros.pagina = 1;
    this.cargar();
  }

  limpiarFiltros(): void {
    this.filtros = { entidadTipo: '', accion: '', ejecutorEmail: '', fechaDesde: '', fechaHasta: '', pagina: 1, tamanoPagina: 50 };
    this.cargar();
  }

  irAPagina(pagina: number): void {
    this.filtros.pagina = pagina;
    this.cargar();
  }

  parsearJson(json: string | null): string {
    if (!json) return '—';
    try {
      return JSON.stringify(JSON.parse(json), null, 2);
    } catch {
      return json;
    }
  }

  formatearFecha(iso: string): string {
    return new Date(iso).toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'medium' });
  }

  badgeAccion(accion: string): string {
    const map: Record<string, string> = {
      CREAR: 'badge-crear',
      MODIFICAR: 'badge-modificar',
      DESACTIVAR: 'badge-desactivar',
      REACTIVAR: 'badge-reactivar'
    };
    return map[accion] ?? '';
  }

  get paginas(): number[] {
    const total = this.resultado()?.totalPaginas ?? 0;
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  irAlDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}
