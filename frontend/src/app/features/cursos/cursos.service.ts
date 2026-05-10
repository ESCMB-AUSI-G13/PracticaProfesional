import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface Curso {
  id:               number;
  anio:             number;
  anioLectivo:      number;
  comision:         string;
  cupo:             number;
  estado:           string;
  preceptorId:      number;
  preceptorNombre:  string;
}

export interface CrearCursoRequest {
  anio:        number;
  anioLectivo: number;
  comision:    string;
  cupo:        number;
  preceptorId: number;
}

export interface ModificarCursoRequest {
  comision: string;
  cupo:     number;
}

@Injectable({ providedIn: 'root' })
export class CursosService {
  private readonly apiUrl = `${environment.apiUrl}/cursos`;

  constructor(private http: HttpClient) {}

  listar(): Observable<Curso[]> {
    return this.http.get<Curso[]>(this.apiUrl);
  }

  crear(dto: CrearCursoRequest): Observable<Curso> {
    return this.http.post<Curso>(this.apiUrl, dto);
  }

  modificar(id: number, dto: ModificarCursoRequest): Observable<Curso> {
    return this.http.put<Curso>(`${this.apiUrl}/${id}`, dto);
  }

  cerrar(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/cerrar`, {});
  }

  reactivar(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/reactivar`, {});
  }
}
