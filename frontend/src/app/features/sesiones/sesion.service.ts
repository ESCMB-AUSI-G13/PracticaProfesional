import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

const HEARTBEAT_MS   = 30_000;
const TOKEN_KEY      = 'auth_token';
const CERRAR_URL     = 'http://localhost:5000/api/sesiones/heartbeat';

@Injectable({ providedIn: 'root' })
export class SesionService implements OnDestroy {
  private readonly apiUrl = 'http://localhost:5000/api/sesiones';
  private intervalo: ReturnType<typeof setInterval> | null = null;

  // Referencia guardada para poder remover el listener exacto
  private readonly onBeforeUnload = () => this.enviarCierreConBeacon();

  constructor(private http: HttpClient) {}

  /** Inicia el heartbeat periódico + listener de cierre de ventana. Idempotente. */
  iniciarHeartbeat(): void {
    if (this.intervalo !== null) return;

    this.ping();
    this.intervalo = setInterval(() => this.ping(), HEARTBEAT_MS);

    // Registra el cierre de pestaña/ventana
    window.addEventListener('beforeunload', this.onBeforeUnload);
  }

  /** Detiene el heartbeat, remueve el listener y notifica al backend. */
  detenerHeartbeat(): void {
    window.removeEventListener('beforeunload', this.onBeforeUnload);

    if (this.intervalo !== null) {
      clearInterval(this.intervalo);
      this.intervalo = null;
    }

    // Logout manual: usa HttpClient normalmente (hay tiempo para completar)
    this.http.delete(CERRAR_URL).subscribe({ error: () => {} });
  }

  /** IDs de usuarios activos — solo para Dirección. */
  obtenerActivas(): Observable<number[]> {
    return this.http.get<number[]>(`${this.apiUrl}/activas`);
  }

  private ping(): void {
    this.http.post(`${this.apiUrl}/heartbeat`, {}).subscribe({ error: () => {} });
  }

  /**
   * Llamado por beforeunload (cierre de pestaña/navegador).
   * Usa fetch con keepalive:true, el único mecanismo garantizado
   * por el navegador para enviar requests al cerrar la página.
   */
  private enviarCierreConBeacon(): void {
    const token = localStorage.getItem(TOKEN_KEY);
    if (!token) return;

    fetch(CERRAR_URL, {
      method: 'DELETE',
      keepalive: true,                          // sobrevive al cierre de la página
      headers: { Authorization: `Bearer ${token}` }
    }).catch(() => {});
  }

  ngOnDestroy(): void {
    this.detenerHeartbeat();
  }
}
