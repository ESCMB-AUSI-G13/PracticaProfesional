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

// ── Modelos RR-05 ────────────────────────────────────────────────────────────

export interface FilaComparativoComision {
  cursoAnio:            number;
  comision:             string;
  totalInscriptos:      number;
  totalConNota:         number;
  aprobados:            number;
  desaprobados:         number;
  promedioGeneral:      number | null;
  porcentajeAprobacion: number;
}

export interface ReporteComparativoComisiones {
  generadoEn:    string;
  materiaNombre: string | null;
  anioFiltro:    number | null;
  comisiones:    FilaComparativoComision[];
}

// ── Modelos RR-06 ────────────────────────────────────────────────────────────

export interface PuntoEvolucionNota {
  periodo:              string;
  totalEvaluados:       number;
  aprobados:            number;
  desaprobados:         number;
  promedioGeneral:      number | null;
  porcentajeAprobacion: number;
}

export interface ReporteEvolucionNotas {
  generadoEn:    string;
  materiaNombre: string | null;
  anioFiltro:    number | null;
  evolucion:     PuntoEvolucionNota[];
}

// ── Modelos RR-07 ────────────────────────────────────────────────────────────

export interface FilaPromedioCatedra {
  espacioCurricularId:   number;
  materiaNombre:         string;
  docenteNombreCompleto: string;
  comision:              string;
  cursoAnio:             number;
  totalEstudiantes:      number;
  totalConNota:          number;
  aprobados:             number;
  desaprobados:          number;
  promedioGeneral:       number | null;
  porcentajeAprobacion:  number;
}

export interface ReportePromediosCatedra {
  generadoEn: string;
  anioFiltro: number | null;
  catedras:   FilaPromedioCatedra[];
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

  /** RR-05: Comparativo de rendimiento entre comisiones. */
  obtenerComparativoComisiones(materiaId?: number, anio?: number): Observable<ReporteComparativoComisiones> {
    const params: Record<string, string> = {};
    if (materiaId) params['materiaId'] = String(materiaId);
    if (anio)      params['anio']      = String(anio);
    return this.http.get<ReporteComparativoComisiones>(`${this.apiUrl}/rendimiento/comisiones`, { params });
  }

  /** RR-06: Evolución de notas en el tiempo. */
  obtenerEvolucionNotas(materiaId?: number, anio?: number): Observable<ReporteEvolucionNotas> {
    const params: Record<string, string> = {};
    if (materiaId) params['materiaId'] = String(materiaId);
    if (anio)      params['anio']      = String(anio);
    return this.http.get<ReporteEvolucionNotas>(`${this.apiUrl}/rendimiento/evolucion`, { params });
  }

  /** RR-07: Promedios por cátedra. */
  obtenerPromediosCatedra(anio?: number, cursoId?: number): Observable<ReportePromediosCatedra> {
    const params: Record<string, string> = {};
    if (anio)    params['anio']    = String(anio);
    if (cursoId) params['cursoId'] = String(cursoId);
    return this.http.get<ReportePromediosCatedra>(`${this.apiUrl}/rendimiento/catedras`, { params });
  }
}
