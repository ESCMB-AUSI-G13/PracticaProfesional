import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// ── Modelos RR-08 ────────────────────────────────────────────────────────────

export interface FiltroInasistencias {
  cursoId?:    number;
  materiaId?:  number;
  fechaDesde?: string;
  fechaHasta?: string;
  soloAusencias: boolean;
}

export interface RegistroInasistencia {
  estudianteId:  number;
  legajo:        string;
  nombreCompleto: string;
  materia:       string;
  curso:         string;
  fecha:         string;
  tipoAsistencia: string;
  motivo?:       string;
}

export interface ReporteInasistencias {
  generadoEn:               string;
  totalRegistros:            number;
  totalAusentes:             number;
  totalAusentesJustificados: number;
  registros:                 RegistroInasistencia[];
}

// ── Modelos RR-09 ────────────────────────────────────────────────────────────

export interface ResumenAsistenciaMateria {
  materiaId:              number;
  materia:                string;
  curso:                  string;
  totalClases:            number;
  presentes:              number;
  ausentesJustificados:   number;
  ausentesInjustificados: number;
  porcentajePresencia:    number;
  porcentajeAusencias:    number;
  enRiesgoRegularidad:    boolean;
  perdioRegularidad:      boolean;
}

export interface ControlLegajo {
  estudianteId:                      number;
  legajo:                            string;
  nombreCompleto:                    string;
  condicionAcademica:                string;
  anio:                              number;
  fechaDeIngreso:                    string;
  asistenciasPorMateria:             ResumenAsistenciaMateria[];
  totalClasesGlobal:                 number;
  totalPresentesGlobal:              number;
  totalAusentesJustificadosGlobal:   number;
  totalAusentesInjustificadosGlobal: number;
  porcentajePresenciaGlobal:         number;
  materiasEnRiesgo:                  number;
  materiasConRegularidadPerdida:     number;
  generadoEn:                        string;
}

// ── Servicio ─────────────────────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class ReportesService {
  private readonly apiUrl = 'http://localhost:5000/api/reportes';

  constructor(private http: HttpClient) {}

  /** RR-08: Reporte detallado de inasistencias con filtros. */
  obtenerInasistencias(filtro: FiltroInasistencias): Observable<ReporteInasistencias> {
    return this.http.post<ReporteInasistencias>(`${this.apiUrl}/inasistencias`, filtro);
  }

  /** RR-09: Control individual de asistencia por legajo. */
  obtenerControlPorLegajo(legajo: string): Observable<ControlLegajo> {
    return this.http.get<ControlLegajo>(`${this.apiUrl}/control-legajo/${encodeURIComponent(legajo)}`);
  }
}
