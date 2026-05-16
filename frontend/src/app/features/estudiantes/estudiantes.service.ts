import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface EstudianteBusqueda {
  id:       number;
  nombre:   string;
  apellido: string;
  legajo:   string;
}

export interface Estudiante {
  id:            number;
  usuarioId:     number;
  dni:           string;
  legajo:        string;
  email:         string;
  nombre:        string;
  apellido:      string;
  anio:          number;
  carreraId:     number;
  carreraNombre: string;
  condicion:     string;
  fechaDeIngreso: string;
  activo:        boolean;
  fechaCreacion: string;
}

export interface CrearEstudianteRequest {
  dni:           string;
  email:         string;
  nombre:        string;
  apellido:      string;
  password:      string;
  anio:          number;
  carreraId:     number;
  fechaDeIngreso: string;
}

export interface ModificarEstudianteRequest {
  nombre:    string;
  apellido:  string;
  email:     string;
  anio:      number;
  carreraId: number;
  condicion: string;
}

@Injectable({ providedIn: 'root' })
export class EstudiantesService {
  private readonly apiUrl = `${environment.apiUrl}/estudiantes`;

  constructor(private http: HttpClient) {}

  listar(): Observable<Estudiante[]> {
    return this.http.get<Estudiante[]>(this.apiUrl);
  }

  buscarParaAutocompletar(): Observable<EstudianteBusqueda[]> {
    return this.http.get<EstudianteBusqueda[]>(`${this.apiUrl}/buscar`);
  }

  crear(dto: CrearEstudianteRequest): Observable<Estudiante> {
    return this.http.post<Estudiante>(this.apiUrl, dto);
  }

  modificar(usuarioId: number, dto: ModificarEstudianteRequest): Observable<Estudiante> {
    return this.http.put<Estudiante>(`${this.apiUrl}/${usuarioId}`, dto);
  }

  desactivar(usuarioId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${usuarioId}`);
  }

  reactivar(usuarioId: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${usuarioId}/reactivar`, {});
  }
}
