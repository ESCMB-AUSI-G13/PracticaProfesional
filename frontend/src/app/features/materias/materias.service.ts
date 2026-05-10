import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface Materia {
  id:           number;
  codigo:       string;
  nombre:       string;
  carreraId:    number;
  carreraNombre: string;
}

export interface CrearMateriaRequest {
  nombre:    string;
  carreraId: number;
}

export interface ModificarMateriaRequest {
  nombre:    string;
  carreraId: number;
}

@Injectable({ providedIn: 'root' })
export class MateriasService {
  private readonly apiUrl = `${environment.apiUrl}/materias`;

  constructor(private http: HttpClient) {}

  listar(): Observable<Materia[]> {
    return this.http.get<Materia[]>(this.apiUrl);
  }

  listarMiCarrera(): Observable<Materia[]> {
    return this.http.get<Materia[]>(`${this.apiUrl}/mi-carrera`);
  }

  crear(dto: CrearMateriaRequest): Observable<Materia> {
    return this.http.post<Materia>(this.apiUrl, dto);
  }

  modificar(id: number, dto: ModificarMateriaRequest): Observable<Materia> {
    return this.http.put<Materia>(`${this.apiUrl}/${id}`, dto);
  }
}
