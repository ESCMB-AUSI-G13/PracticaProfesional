import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Docente {
  id: number;
  usuarioId: number;
  dni: string;
  legajo: string;
  email: string;
  nombre: string;
  apellido: string;
  telefono: string;
  categoria: string;
  activo: boolean;
  fechaCreacion: string;
}

export interface CrearDocenteRequest {
  dni: string;
  legajo: string;
  email: string;
  nombre: string;
  apellido: string;
  password: string;
  telefono: string;
  categoria: string;
}

export interface ModificarDocenteRequest {
  nombre: string;
  apellido: string;
  email: string;
  telefono: string;
  categoria: string;
}

@Injectable({ providedIn: 'root' })
export class DocentesService {
  private readonly apiUrl = 'http://localhost:5000/api/docentes';

  constructor(private http: HttpClient) {}

  listar(): Observable<Docente[]> {
    return this.http.get<Docente[]>(this.apiUrl);
  }

  crear(dto: CrearDocenteRequest): Observable<Docente> {
    return this.http.post<Docente>(this.apiUrl, dto);
  }

  modificar(usuarioId: number, dto: ModificarDocenteRequest): Observable<Docente> {
    return this.http.put<Docente>(`${this.apiUrl}/${usuarioId}`, dto);
  }

  desactivar(usuarioId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${usuarioId}`);
  }

  reactivar(usuarioId: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${usuarioId}/reactivar`, {});
  }
}
