import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LogsSeguridadService, LogSeguridadDto, LogSeguridadFiltros } from '../logs-seguridad.service';
import { PaginadoDto } from '../../auditoria/auditoria.service';

@Component({
  selector: 'app-panel-logs-seguridad',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-logs-seguridad.component.html',
  styleUrl:    './panel-logs-seguridad.component.scss'
})
export class PanelLogsSeguridadComponent implements OnInit {
  resultado = signal<PaginadoDto<LogSeguridadDto> | null>(null);
  cargando  = signal(true);
  error     = signal<string | null>(null);

  filtros: LogSeguridadFiltros = {
    email: '',
    soloFallidos: null,
    fechaDesde: '',
    fechaHasta: '',
    pagina: 1,
    tamanoPagina: 50
  };

  constructor(
    private service: LogsSeguridadService,
    private router: Router
  ) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);

    const f: LogSeguridadFiltros = {
      ...this.filtros,
      email:       this.filtros.email       || undefined,
      fechaDesde:  this.filtros.fechaDesde  || undefined,
      fechaHasta:  this.filtros.fechaHasta  || undefined,
      soloFallidos: this.filtros.soloFallidos ?? undefined
    };

    this.service.listar(f).subscribe({
      next:  (d) => { this.resultado.set(d); this.cargando.set(false); },
      error: ()  => { this.error.set('Error al cargar los logs de seguridad.'); this.cargando.set(false); }
    });
  }

  aplicarFiltros(): void { this.filtros.pagina = 1; this.cargar(); }

  limpiarFiltros(): void {
    this.filtros = { email: '', soloFallidos: null, fechaDesde: '', fechaHasta: '', pagina: 1, tamanoPagina: 50 };
    this.cargar();
  }

  irAPagina(p: number): void { this.filtros.pagina = p; this.cargar(); }

  formatearFecha(iso: string): string {
    return new Date(iso).toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'medium' });
  }

  acortarUserAgent(ua: string): string {
    return ua.length > 80 ? ua.slice(0, 80) + '…' : ua;
  }

  get paginas(): number[] {
    const total = this.resultado()?.totalPaginas ?? 0;
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  // Contadores rápidos
  get totalExitosos(): number  { return this.resultado()?.items.filter(i =>  i.exitoso).length ?? 0; }
  get totalFallidos(): number  { return this.resultado()?.items.filter(i => !i.exitoso).length ?? 0; }

  irAlDashboard(): void { this.router.navigate(['/dashboard']); }
}
