import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CalendarioService, EventoCalendario, etiquetaTipo } from '../calendario.service';

@Component({
  selector: 'app-lista-calendario',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './lista-calendario.component.html',
  styleUrl: './lista-calendario.component.scss'
})
export class ListaCalendarioComponent implements OnInit {
  anioFiltro   = signal(new Date().getFullYear());
  eventos      = signal<EventoCalendario[]>([]);
  cargando     = signal(true);
  error        = signal<string | null>(null);
  eliminandoId = signal<number | null>(null);

  aniosDisponibles = [2025, 2026, 2027];

  constructor(
    private calendarioService: CalendarioService,
    private router: Router
  ) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.calendarioService.listar(this.anioFiltro()).subscribe({
      next: data => { this.eventos.set(data); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar el calendario.'); this.cargando.set(false); }
    });
  }

  irACrear(): void { this.router.navigate(['/calendario/nuevo']); }
  irAEditar(id: number): void { this.router.navigate(['/calendario', id, 'editar']); }

  eliminar(id: number): void {
    if (!confirm('¿Eliminar este evento del calendario?')) return;
    this.eliminandoId.set(id);
    this.calendarioService.eliminar(id).subscribe({
      next: () => { this.eventos.update(list => list.filter(e => e.id !== id)); this.eliminandoId.set(null); },
      error: () => { this.error.set('Error al eliminar el evento.'); this.eliminandoId.set(null); }
    });
  }

  etiqueta(tipo: string): string { return etiquetaTipo(tipo); }

  formatFecha(f: string): string {
    return new Date(f).toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  claseTipo(tipo: string): string {
    const mapa: Record<string, string> = {
      PeriodoExamen:         'badge-examen',
      InscripcionExamen:     'badge-inscr-examen',
      InscripcionMateria:    'badge-inscr-materia',
      InicioClases:          'badge-inicio',
      FinClases:             'badge-fin',
      FechaLimiteCargaNotas: 'badge-limite',
      Feriado:               'badge-feriado',
      Otro:                  'badge-otro',
    };
    return mapa[tipo] ?? 'badge-otro';
  }
}
