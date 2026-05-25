import { Injectable, signal, computed, inject, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface NotificacionDto {
  id: number;
  titulo: string;
  mensaje: string;
  leida: boolean;
  fechaCreacion: string;
  tipo: string | null;
}

@Injectable({ providedIn: 'root' })
export class NotificacionesService implements OnDestroy {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/notificaciones`;
  private readonly INTERVALO_MS = 60_000;

  private readonly _notificaciones = signal<NotificacionDto[]>([]);
  readonly notificaciones = this._notificaciones.asReadonly();
  readonly noLeidas = computed(() => this._notificaciones().filter(n => !n.leida).length);

  private intervalo: ReturnType<typeof setInterval> | null = null;

  iniciarPolling(): void {
    if (this.intervalo) return;
    this.cargar();
    this.intervalo = setInterval(() => this.cargar(), this.INTERVALO_MS);
  }

  detenerPolling(): void {
    if (this.intervalo) {
      clearInterval(this.intervalo);
      this.intervalo = null;
    }
    this._notificaciones.set([]);
  }

  cargar(): void {
    this.http.get<NotificacionDto[]>(this.apiUrl).subscribe({
      next: data => this._notificaciones.set(data),
      error: () => { /* silencioso, no interrumpe la UI */ }
    });
  }

  marcarLeida(id: number): void {
    this.http.patch(`${this.apiUrl}/${id}/leida`, {}).subscribe(() => {
      this._notificaciones.update(ns =>
        ns.map(n => n.id === id ? { ...n, leida: true } : n)
      );
    });
  }

  marcarTodasLeidas(): void {
    this.http.patch(`${this.apiUrl}/marcar-todas-leidas`, {}).subscribe(() => {
      this._notificaciones.update(ns => ns.map(n => ({ ...n, leida: true })));
    });
  }

  ngOnDestroy(): void {
    this.detenerPolling();
  }
}
