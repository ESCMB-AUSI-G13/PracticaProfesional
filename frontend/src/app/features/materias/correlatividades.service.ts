import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Correlatividad {
  id:                number;
  materiaDestinoId:  number;
  materiaRequisitoId: number;
  nombreRequisito:   string;
  tipoRequerimiento: string;
  condicionAcademica: string;
}

export interface CrearCorrelativiadadRequest {
  materiaDestinoId:   number;
  materiaRequisitoId: number;
  tipoRequerimiento:  string;
  condicionAcademica: 1 | 2;
}

@Injectable({ providedIn: 'root' })
export class CorrelativiadadesService {
  private readonly apiUrl = 'http://localhost:5000/api/correlatividades';

  constructor(private http: HttpClient) {}

  listarPorMateria(materiaId: number): Observable<Correlatividad[]> {
    return this.http.get<Correlatividad[]>(`${this.apiUrl}?materiaId=${materiaId}`);
  }

  crear(dto: CrearCorrelativiadadRequest): Observable<Correlatividad> {
    return this.http.post<Correlatividad>(this.apiUrl, dto);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
