import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ExamenFinalDisponible {
  id:            number;
  materiaId:     number;
  materiaNombre: string;
  materiaCodigo: string;
  fechaExamen:   string;
  horario:       string;
  cupo:          number;
  tipoExamen:    string;
  yaInscripto:   boolean;
}

@Injectable({ providedIn: 'root' })
export class MisExamenesService {
  private readonly apiUrl = 'http://localhost:5000/api/examenes';

  constructor(private http: HttpClient) {}

  listarFinalesDisponibles(): Observable<ExamenFinalDisponible[]> {
    return this.http.get<ExamenFinalDisponible[]>(`${this.apiUrl}/mis-finales`);
  }

  inscribirse(examenId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${examenId}/inscripciones`, {});
  }
}
