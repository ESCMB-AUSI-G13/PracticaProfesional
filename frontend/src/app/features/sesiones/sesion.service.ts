import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

const HEARTBEAT_MS = 30_000;

@Injectable({ providedIn: 'root' })
export class SesionService implements OnDestroy {
  private readonly apiUrl = 'http://localhost:5000/api/sesiones';
  private intervalo: ReturnType<typeof setInterval> | null = null;

  constructor(private http: HttpClient) {}

  /** Inicia el heartbeat periódico. Idempotente. */
  iniciarHeartbeat(): void {
    if (this.intervalo !== null) return;
    this.ping();
    this.intervalo = setInterval(() => this.ping(), HEARTBEAT_MS);
  }

  /** Detiene el heartbeat y notifica al backend para desconexión inmediata. */
  detenerHeartbeat(): void {
    if (this.intervalo !== null) {
      clearInterval(this.intervalo);
      this.intervalo = null;
    }
    this.http.delete(`${this.apiUrl}/heartbeat`).subscribe({ error: () => {} });
  }

  /** IDs de usuarios activos — solo para Dirección. */
  obtenerActivas(): Observable<number[]> {
    return this.http.get<number[]>(`${this.apiUrl}/activas`);
  }

  private ping(): void {
    this.http.post(`${this.apiUrl}/heartbeat`, {}).subscribe({ error: () => {} });
  }

  ngOnDestroy(): void {
    this.detenerHeartbeat();
  }
}
