import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PreguntaEncuesta {
  id:            number;
  texto:         string;
  orden:         number;
  tipoPregunta:  'EscalaLikert' | 'TextoLibre';
  esObligatoria: boolean;
}

export interface EncuestaDto {
  id:           number;
  titulo:       string;
  descripcion:  string | null;
  tipo:         'EvaluacionDocente' | 'SatisfaccionGeneral';
  materiaId:    number | null;
  cicloLectivo: number;
  activa:       boolean;
  preguntas:    PreguntaEncuesta[];
}

export interface ItemRespuestaRequest {
  preguntaId:    number;
  valorNumerico: number | null;
  textoLibre:    string | null;
}

export interface ResponderEncuestaRequest {
  encuestaId: number;
  items:      ItemRespuestaRequest[];
}

export interface CrearEncuestaRequest {
  titulo:       string;
  descripcion:  string | null;
  tipo:         string;
  cicloLectivo: number;
  materiaId:    number | null;
}

export interface AgregarPreguntaRequest {
  encuestaId:    number;
  texto:         string;
  orden:         number;
  tipoPregunta:  string;
  esObligatoria: boolean;
}

// ── Reportes ──────────────────────────────────────────────────────────────────

export interface ResultadoPreguntaDto {
  preguntaId:      number;
  textoPregunta:   string;
  totalRespuestas: number;
  promedioLikert:  number | null;
  textosLibres:    string[];
}

export interface ReporteSatisfaccionDto {
  encuestaId:            number;
  encuestaTitulo:        string;
  totalRespuestas:       number;
  promedioGlobal:        number | null;
  resultadosPorPregunta: ResultadoPreguntaDto[];
  evolucionMensual:      { periodo: string; totalRespuestas: number; promedioGeneral: number | null }[];
  generadoEn:            string;
}

export interface FilaComparativoEncuestaDto {
  encuestaId:      number;
  titulo:          string;
  tipo:            string;
  cicloLectivo:    number;
  totalRespuestas: number;
  promedioGeneral: number | null;
}

export interface ReporteComparativoEncuestasDto {
  generadoEn: string;
  encuestas:  FilaComparativoEncuestaDto[];
}

@Injectable({ providedIn: 'root' })
export class EncuestasService {
  private readonly api = `${environment.apiUrl}/encuestas`;

  constructor(private http: HttpClient) {}

  // ── Estudiante ─────────────────────────────────────────────────────────────
  obtenerPendiente(): Observable<EncuestaDto | null> {
    return this.http.get<EncuestaDto | null>(`${this.api}/pendiente`);
  }

  responder(dto: ResponderEncuestaRequest): Observable<void> {
    return this.http.post<void>(`${this.api}/responder`, dto);
  }

  // ── Dirección — gestión ───────────────────────────────────────────────────
  listar(): Observable<EncuestaDto[]> {
    return this.http.get<EncuestaDto[]>(this.api);
  }

  crear(dto: CrearEncuestaRequest): Observable<EncuestaDto> {
    return this.http.post<EncuestaDto>(this.api, dto);
  }

  agregarPregunta(dto: AgregarPreguntaRequest): Observable<PreguntaEncuesta> {
    return this.http.post<PreguntaEncuesta>(`${this.api}/preguntas`, dto);
  }

  activar(id: number): Observable<void> {
    return this.http.patch<void>(`${this.api}/${id}/activar`, {});
  }

  desactivar(id: number): Observable<void> {
    return this.http.patch<void>(`${this.api}/${id}/desactivar`, {});
  }

  // ── Dirección — reportes ──────────────────────────────────────────────────
  obtenerResultados(id: number): Observable<ReporteSatisfaccionDto> {
    return this.http.get<ReporteSatisfaccionDto>(`${this.api}/${id}/resultados`);
  }

  obtenerComparativo(): Observable<ReporteComparativoEncuestasDto> {
    return this.http.get<ReporteComparativoEncuestasDto>(`${this.api}/comparativo`);
  }
}
