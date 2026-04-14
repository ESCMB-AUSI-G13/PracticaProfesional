import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { SesionService } from '../../sesiones/sesion.service';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  nombreCompleto: string;
  rol: string;
  expiracion: string;
}

// Roles que puede ver cada rol (hacia abajo en la jerarquía)
const VISTAS_PERMITIDAS: Record<string, string[]> = {
  Direccion: ['Docente', 'Preceptor', 'Estudiante'],
  Docente:   ['Estudiante'],
  Preceptor: [],
  Estudiante: []
};

const TOKEN_KEY = 'auth_token';
const USER_KEY  = 'auth_user';
const ROL_VISTA_KEY = 'rol_vista';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = 'http://localhost:5000/api/auth';
  private readonly auditoriaUrl = 'http://localhost:5000/api/auditoria';

  private _usuario   = signal<AuthResponse | null>(this.cargarUsuarioGuardado());
  private _rolVista  = signal<string | null>(sessionStorage.getItem(ROL_VISTA_KEY));

  readonly usuario         = this._usuario.asReadonly();
  readonly estaAutenticado = computed(() => this._usuario() !== null);
  readonly rol             = computed(() => this._usuario()?.rol ?? null);

  /** Rol activo para filtrar UI. Si está en modo vista, es el rol suplantado; si no, el propio. */
  readonly rolVista = computed(() => this._rolVista() ?? this.rol());

  /** true cuando la vista activa es distinta al rol real */
  readonly enModoVista = computed(() =>
    this._rolVista() !== null && this._rolVista() !== this.rol()
  );

  readonly vistasDisponibles = computed(() => {
    const r = this.rol();
    return r ? (VISTAS_PERMITIDAS[r] ?? []) : [];
  });

  constructor(
    private http: HttpClient,
    private router: Router,
    private sesionService: SesionService
  ) {}

  login(credenciales: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credenciales).pipe(
      tap(response => {
        localStorage.setItem(TOKEN_KEY, response.token);
        localStorage.setItem(USER_KEY, JSON.stringify(response));
        this._usuario.set(response);
        // Limpia cualquier modo vista de sesión anterior
        sessionStorage.removeItem(ROL_VISTA_KEY);
        this._rolVista.set(null);
        // Registra presencia en el servidor
        this.sesionService.iniciarHeartbeat();
      })
    );
  }

  activarVista(rolObjetivo: string): void {
    const rolReal = this.rol();
    if (!rolReal || !VISTAS_PERMITIDAS[rolReal]?.includes(rolObjetivo)) return;

    sessionStorage.setItem(ROL_VISTA_KEY, rolObjetivo);
    this._rolVista.set(rolObjetivo);

    this.http.post(`${this.auditoriaUrl}/cambio-rol`, {
      rolOriginal: rolReal,
      rolVista: rolObjetivo,
      accion: 'ACTIVAR'
    }).subscribe();
  }

  restaurarRol(): void {
    const rolReal = this.rol();
    const rolVista = this._rolVista();
    if (!rolReal || !rolVista) return;

    sessionStorage.removeItem(ROL_VISTA_KEY);
    this._rolVista.set(null);

    this.http.post(`${this.auditoriaUrl}/cambio-rol`, {
      rolOriginal: rolReal,
      rolVista: rolVista,
      accion: 'RESTAURAR'
    }).subscribe();
  }

  logout(): void {
    if (this.enModoVista()) this.restaurarRol();
    // Notifica al backend que el usuario se desconectó
    this.sesionService.detenerHeartbeat();
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    sessionStorage.removeItem(ROL_VISTA_KEY);
    this._usuario.set(null);
    this._rolVista.set(null);
    this.router.navigate(['/login']);
  }

  solicitarRestablecimiento(email: string): Observable<{ mensaje: string; enlace?: string }> {
    return this.http.post<{ mensaje: string; enlace?: string }>(`${this.apiUrl}/olvide-password`, { email });
  }

  restablecerPassword(token: string, nuevaPassword: string, confirmarPassword: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/reset-password`, { token, nuevaPassword, confirmarPassword });
  }

  obtenerToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  private cargarUsuarioGuardado(): AuthResponse | null {
    try {
      const json = localStorage.getItem(USER_KEY);
      if (!json) return null;
      const usuario: AuthResponse = JSON.parse(json);
      if (new Date(usuario.expiracion) <= new Date()) {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
        return null;
      }
      return usuario;
    } catch {
      return null;
    }
  }
}
