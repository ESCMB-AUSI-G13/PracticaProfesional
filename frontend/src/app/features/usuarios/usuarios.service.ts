import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface Usuario {
  id: number;
  dni: string;
  legajo: string;
  email: string;
  nombre: string;
  apellido: string;
  rol: string;
  activo: boolean;
  fechaCreacion: string;
}

export interface CrearUsuarioRequest {
  dni: string;
  email: string;
  nombre: string;
  apellido: string;
  password: string;
  rol: string;
}

export interface ModificarUsuarioRequest {
  nombre: string;
  apellido: string;
  email: string;
  rol: string;
}

@Injectable({ providedIn: 'root' })
export class UsuariosService {
  private readonly apiUrl = `${environment.apiUrl}/usuarios`;

  constructor(private http: HttpClient) {}

  listar(rol?: string): Observable<Usuario[]> {
    if (rol) {
      return this.http.get<Usuario[]>(this.apiUrl, { params: { rol } });
    }
    return this.http.get<Usuario[]>(this.apiUrl);
  }

  crear(dto: CrearUsuarioRequest): Observable<Usuario> {
    return this.http.post<Usuario>(this.apiUrl, dto);
  }

  modificar(id: number, dto: ModificarUsuarioRequest): Observable<Usuario> {
    return this.http.put<Usuario>(`${this.apiUrl}/${id}`, dto);
  }

  desactivar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  reactivar(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/reactivar`, {});
  }

  cambiarClave(id: number, nuevaClave: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/clave`, { nuevaClave });
  }
}
