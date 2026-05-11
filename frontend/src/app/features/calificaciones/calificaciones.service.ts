import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface InscripcionExamenDto {
  id:                      number;
  estudianteId:            number;
  estudianteNombreCompleto: string;
  estudianteLegajo:        string;
  examenId:                number;
  tipoExamen:              string;
  materiaNombre:           string;
  notaValor:               number | null;
  esAprobado:              boolean | null;
  estado:                  string;
  fechaInscripcion:        string;
}

export interface NotaExamenResultDto {
  inscripcionExamenId:     number;
  estudianteId:            number;
  estudianteNombreCompleto: string;
  estudianteLegajo:        string;
  examenId:                number;
  tipoExamen:              string;
  materiaNombre:           string;
  notaValor:               number;
  esAprobado:              boolean;
  estado:                  string;
}

export interface CambioNotaDto {
  id:            number;
  accion:        string;
  valorAnterior: string | null;
  valorNuevo:    string | null;
  ejecutorEmail: string;
  timestamp:     string;
}

export interface ExamenResumen {
  id:            number;
  materiaId:     number;
  materiaNombre: string;
  fechaExamen:   string;
  horario:       string;
  cupo:          number;
  tipoExamen:    string;
}

@Injectable({ providedIn: 'root' })
export class CalificacionesService {
  private readonly apiUrl        = `${environment.apiUrl}/calificaciones`;
  private readonly examenesUrl   = `${environment.apiUrl}/examenes`;

  constructor(private http: HttpClient) {}

  /** Lista todos los exámenes accesibles para el docente autenticado. */
  listarExamenes(): Observable<ExamenResumen[]> {
    return this.http.get<ExamenResumen[]>(this.examenesUrl);
  }

  /** Obtiene el acta de inscriptos a un examen con notas ya cargadas (si las hay). */
  listarInscripciones(examenId: number): Observable<InscripcionExamenDto[]> {
    return this.http.get<InscripcionExamenDto[]>(
      `${this.apiUrl}/examenes/${examenId}/inscripciones`
    );
  }

  /** Carga la nota de un estudiante en una inscripción a examen. */
  cargarNota(inscripcionExamenId: number, nota: number): Observable<NotaExamenResultDto> {
    return this.http.put<NotaExamenResultDto>(
      `${this.apiUrl}/examenes/inscripciones/${inscripcionExamenId}/nota`,
      { nota }
    );
  }

  /** Rectifica una nota ya cargada. Requiere motivo. */
  rectificarNota(inscripcionExamenId: number, nuevaNota: number, motivo: string): Observable<NotaExamenResultDto> {
    return this.http.put<NotaExamenResultDto>(
      `${this.apiUrl}/examenes/inscripciones/${inscripcionExamenId}/nota/rectificar`,
      { nuevaNota, motivo }
    );
  }

  /** Obtiene el historial completo de cambios de nota para una inscripción. */
  obtenerHistorial(inscripcionExamenId: number): Observable<CambioNotaDto[]> {
    return this.http.get<CambioNotaDto[]>(
      `${this.apiUrl}/examenes/inscripciones/${inscripcionExamenId}/historial`
    );
  }
}
