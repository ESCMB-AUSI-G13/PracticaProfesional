import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface Preceptor {
  id: number;
  usuarioId: number;
  dni: string;
  legajo: string;
  email: string;
  nombre: string;
  apellido: string;
  telefono: string;
  turno: string;
  activo: boolean;
  fechaCreacion: string;
}

export interface CrearPreceptorRequest {
  dni: string;
  email: string;
  nombre: string;
  apellido: string;
  password: string;
  telefono: string;
  turno: string;
}

export interface ModificarPreceptorRequest {
  nombre: string;
  apellido: string;
  email: string;
  telefono: string;
  turno: string;
}

@Injectable({ providedIn: 'root' })
export class PreceptoresService {
  private readonly apiUrl = `${environment.apiUrl}/preceptores`;

  constructor(private http: HttpClient) {}

  listar(): Observable<Preceptor[]> {
    return this.http.get<Preceptor[]>(this.apiUrl);
  }

  crear(dto: CrearPreceptorRequest): Observable<Preceptor> {
    return this.http.post<Preceptor>(this.apiUrl, dto);
  }

  modificar(usuarioId: number, dto: ModificarPreceptorRequest): Observable<Preceptor> {
    return this.http.put<Preceptor>(`${this.apiUrl}/${usuarioId}`, dto);
  }

  desactivar(usuarioId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${usuarioId}`);
  }

  reactivar(usuarioId: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${usuarioId}/reactivar`, {});
  }
}
