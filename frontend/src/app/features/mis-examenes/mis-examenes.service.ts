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

export interface InscripcionExamenResult {
  id:              number;
  estudianteId:    number;
  estudianteNombre: string;
  examenId:        number;
  materiaNombre:   string;
  tipoExamen:      string;
  fechaExamen:     string;
  estado:          string;
}

export interface ComprobanteInscripcionExamen {
  id:                       number;
  estudianteNombreCompleto: string;
  estudianteDni:            string;
  estudianteLegajo:         string;
  materiaCodigo:            string;
  materiaNombre:            string;
  tipoExamen:               string;
  fechaExamen:              string;
  horario:                  string;
  estado:                   string;
  fechaInscripcion:         string;
  fechaEmision:             string;
}

@Injectable({ providedIn: 'root' })
export class MisExamenesService {
  private readonly apiUrl = 'http://localhost:5000/api/examenes';

  constructor(private http: HttpClient) {}

  listarFinalesDisponibles(): Observable<ExamenFinalDisponible[]> {
    return this.http.get<ExamenFinalDisponible[]>(`${this.apiUrl}/mis-finales`);
  }

  inscribirse(examenId: number): Observable<InscripcionExamenResult> {
    return this.http.post<InscripcionExamenResult>(`${this.apiUrl}/${examenId}/inscripciones`, {});
  }

  obtenerComprobante(inscripcionId: number): Observable<ComprobanteInscripcionExamen> {
    return this.http.get<ComprobanteInscripcionExamen>(`${this.apiUrl}/inscripciones/${inscripcionId}/comprobante`);
  }
}
