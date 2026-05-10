import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { InscripcionMateria, ComprobanteInscripcionMateria } from '../inscripciones-materia/inscripciones-materia.service';

export interface AutogestRequest {
  materiaId: number;
  cursoId:   number;
}

export interface InscripcionMateriaResult {
  id:           number;
  estudianteId: number;
  materiaId:    number;
  materiaNombre: string;
  cursoId:      number;
  estado:       string;
  fechaInscripcion: string;
}

export { InscripcionMateria, ComprobanteInscripcionMateria };

@Injectable({ providedIn: 'root' })
export class MisMateriasService {
  private readonly apiUrl = `${environment.apiUrl}/inscripciones`;

  constructor(private http: HttpClient) {}

  listarMisInscripciones(): Observable<InscripcionMateria[]> {
    return this.http.get<InscripcionMateria[]>(`${this.apiUrl}/mis-materias`);
  }

  inscribirse(dto: AutogestRequest): Observable<InscripcionMateriaResult> {
    return this.http.post<InscripcionMateriaResult>(`${this.apiUrl}/mis-materias`, dto);
  }

  obtenerComprobante(id: number): Observable<ComprobanteInscripcionMateria> {
    return this.http.get<ComprobanteInscripcionMateria>(`${this.apiUrl}/materias/${id}/comprobante`);
  }
}
