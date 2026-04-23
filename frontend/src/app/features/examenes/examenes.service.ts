import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Examen {
  id:            number;
  materiaId:     number;
  materiaNombre: string;
  fechaExamen:   string;
  horario:       string;
  cupo:          number;
  tipoExamen:    string;
}

export interface CrearExamenRequest {
  materiaId:   number;
  fechaExamen: string;
  horario:     string;
  cupo:        number;
  tipoExamen:  string;
}

export interface InscripcionExamenResult {
  id:               number;
  estudianteId:     number;
  estudianteNombre: string;
  examenId:         number;
  materiaNombre:    string;
  tipoExamen:       string;
  fechaExamen:      string;
  estado:           string;
}

export const TIPOS_EXAMEN = [
  'ParcialEscrito',
  'ParcialOral',
  'FinalEscrito',
  'FinalOral',
  'Recuperatorio'
];

@Injectable({ providedIn: 'root' })
export class ExamenesService {
  private readonly apiUrl = 'http://localhost:5000/api/examenes';

  constructor(private http: HttpClient) {}

  listar(): Observable<Examen[]> {
    return this.http.get<Examen[]>(this.apiUrl);
  }

  crear(dto: CrearExamenRequest): Observable<Examen> {
    return this.http.post<Examen>(this.apiUrl, dto);
  }

  inscribirEstudiante(examenId: number, estudianteId: number): Observable<InscripcionExamenResult> {
    return this.http.post<InscripcionExamenResult>(`${this.apiUrl}/${examenId}/inscripciones`, { estudianteId });
  }
}
