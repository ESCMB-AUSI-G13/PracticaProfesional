import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Materia {
  id:     number;
  codigo: string;
  nombre: string;
  plan:   string;
}

export interface CrearMateriaRequest {
  codigo: string;
  nombre: string;
  plan:   string;
}

export interface ModificarMateriaRequest {
  nombre: string;
  plan:   string;
}

@Injectable({ providedIn: 'root' })
export class MateriasService {
  private readonly apiUrl = 'http://localhost:5000/api/materias';

  constructor(private http: HttpClient) {}

  listar(): Observable<Materia[]> {
    return this.http.get<Materia[]>(this.apiUrl);
  }

  crear(dto: CrearMateriaRequest): Observable<Materia> {
    return this.http.post<Materia>(this.apiUrl, dto);
  }

  modificar(id: number, dto: ModificarMateriaRequest): Observable<Materia> {
    return this.http.put<Materia>(`${this.apiUrl}/${id}`, dto);
  }
}
