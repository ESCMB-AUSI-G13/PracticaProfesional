import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ReportesService, ControlLegajo } from '../reportes.service';

@Component({
  selector: 'app-control-legajo',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './control-legajo.component.html',
  styleUrl: './control-legajo.component.scss'
})
export class ControlLegajoComponent {
  legajoBusqueda = signal('');
  resultado      = signal<ControlLegajo | null>(null);
  cargando       = signal(false);
  error          = signal<string | null>(null);

  constructor(
    private reportesService: ReportesService,
    private router: Router
  ) {}

  buscar(): void {
    const legajo = this.legajoBusqueda().trim();
    if (!legajo) return;

    this.cargando.set(true);
    this.error.set(null);
    this.resultado.set(null);

    this.reportesService.obtenerControlPorLegajo(legajo).subscribe({
      next: data => {
        this.resultado.set(data);
        this.cargando.set(false);
      },
      error: err => {
        const msg = err.status === 400
          ? `No se encontró ningún estudiante con legajo "${legajo}".`
          : 'Error al obtener el control de asistencia.';
        this.error.set(msg);
        this.cargando.set(false);
      }
    });
  }

  limpiar(): void {
    this.legajoBusqueda.set('');
    this.resultado.set(null);
    this.error.set(null);
  }

  irAlDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}
