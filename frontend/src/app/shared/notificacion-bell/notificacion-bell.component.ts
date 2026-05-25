import { Component, inject, HostListener, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { NotificacionesService } from '../../features/notificaciones/notificaciones.service';

@Component({
  selector: 'app-notificacion-bell',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './notificacion-bell.component.html',
  styleUrl: './notificacion-bell.component.scss'
})
export class NotificacionBellComponent {
  readonly svc = inject(NotificacionesService);
  readonly abierto = signal(false);

  togglePanel(): void {
    this.abierto.update(v => !v);
  }

  marcarLeida(id: number, event: Event): void {
    event.stopPropagation();
    this.svc.marcarLeida(id);
  }

  marcarTodas(): void {
    this.svc.marcarTodasLeidas();
  }

  iconoPorTipo(tipo: string | null): string {
    if (!tipo) return '🔔';
    if (tipo.includes('Riesgo')) return '⚠️';
    if (tipo.includes('Vencimiento')) return '📅';
    return '🔔';
  }

  @HostListener('document:click', ['$event'])
  onClickFuera(event: Event): void {
    const el = event.target as HTMLElement;
    if (!el.closest('.notif-container')) {
      this.abierto.set(false);
    }
  }
}
