import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface EspacioCurricular {
  id:              number;
  materiaId:       number;
  materiaNombre:   string;
  materiaCodigo:   string;
  materiaAnio:     number;
  carreraId:       number;
  carreraNombre:   string;
  docenteId:       number;
  docenteNombre:   string;
  cursoId:         number;
  cursoAnio:       number;
  cursoAnioLectivo: number;
  cursoComision:   string;
}

export interface CrearEspacioCurricularRequest {
  materiaId:        number;
  usuarioDocenteId: number;
  cursoId:          number;
}

@Injectable({ providedIn: 'root' })
export class EspaciosCurricularesService {
  private readonly apiUrl = `${environment.apiUrl}/espacios-curriculares`;

  constructor(private http: HttpClient) {}

  listar(): Observable<EspacioCurricular[]> {
    return this.http.get<EspacioCurricular[]>(this.apiUrl);
  }

  listarMisEspacios(): Observable<EspacioCurricular[]> {
    return this.http.get<EspacioCurricular[]>(`${this.apiUrl}/mis-espacios`);
  }

  crear(dto: CrearEspacioCurricularRequest): Observable<EspacioCurricular> {
    return this.http.post<EspacioCurricular>(this.apiUrl, dto);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
