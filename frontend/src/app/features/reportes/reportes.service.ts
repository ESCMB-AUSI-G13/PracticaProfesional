import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

// ── Modelos RR-08 ────────────────────────────────────────────────────────────

export interface FiltroInasistencias {
  cursoId?:      number;
  anioLectivo?:  number;
  materiaId?:    number;
  fechaDesde?:   string;
  fechaHasta?:   string;
  comision?:     string;
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
  totalPresentes:            number;
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

export interface DetalleCarreraEvolucion {
  carreraId:            number;
  carreraNombre:        string;
  promedio:             number | null;
  porcentajeAprobacion: number;
  totalEvaluados:       number;
}

export interface DistribucionNotaItem {
  nota:     number;
  cantidad: number;
}

export interface PuntoEvolucionNota {
  periodo:              string;
  totalEvaluados:       number;
  aprobados:            number;
  desaprobados:         number;
  promedioGeneral:      number | null;
  porcentajeAprobacion: number;
  porCarrera:           DetalleCarreraEvolucion[];
  distribucionNotas:    DistribucionNotaItem[];
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

// ── Modelos Riesgo Académico ─────────────────────────────────────────────────

export interface RiesgoAcademico {
  estudianteId:            number;
  legajo:                  string;
  nombreCompleto:          string;
  carrera:                 string;
  anioCarrera:             number;
  anioCohorte:             number;
  condicion:               string;
  nivelRiesgo:             'Bajo' | 'Medio' | 'Alto';
  porcentajeInasistencias: number;
  promedioNotas:           number | null;
  materiasReprobadas:      number;
}

export interface ReporteRiesgoAcademico {
  estudiantes: RiesgoAcademico[];
  totalAlto:   number;
  totalMedio:  number;
  totalBajo:   number;
}

// ── Modelos Retención por Cohorte ────────────────────────────────────────────

export interface RetencionCohorte {
  anioCohorte:   number;
  carrera:       string;
  total:         number;
  activos:       number;
  egresados:     number;
  desertores:    number;
  tasaRetencion: number;
  tasaDesercion: number;
  tasaEgreso:    number;
}

export interface ReporteRetencionCohorte {
  cohortes:             RetencionCohorte[];
  totalGeneral:         number;
  tasaRetencionGlobal:  number;
  tasaDesercionGlobal:  number;
}

// ── Modelos RR-01 Tablero Ejecutivo ──────────────────────────────────────────

export interface EvolucionCohorteResumen {
  anioCohorte: number;
  total:       number;
  activos:     number;
  egresados:   number;
  desertores:  number;
}

export interface TableroEjecutivo {
  totalMatriculados:          number;
  totalEgresados:             number;
  totalDesertores:            number;
  totalHistorico:             number;
  riesgoAlto:                 number;
  riesgoMedio:                number;
  riesgoBajo:                 number;
  porcentajeRiesgoAlto:       number;
  tasaRetencionGlobal:        number;
  tasaDesercionGlobal:        number;
  tasaEgresoGlobal:           number;
  promedioNotaGlobal:         number | null;
  porcentajeAprobacionGlobal: number;
  evolucionCohortes:          EvolucionCohorteResumen[];
  generadoEn:                 string;
}

// ── Servicio ─────────────────────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class ReportesService {
  private readonly apiUrl = `${environment.apiUrl}/reportes`;

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
  obtenerEvolucionNotas(
    materiaId?:    number,
    anio?:         number,
    cuatrimestre?: number,
    anioCarrera?:  number,
    tipoExamen?:   number,
    granularidad:  'mensual' | 'cuatrimestral' | 'anual' = 'mensual',
  ): Observable<ReporteEvolucionNotas> {
    const params: Record<string, string> = {};
    if (materiaId)    params['materiaId']    = String(materiaId);
    if (anio)         params['anio']         = String(anio);
    if (cuatrimestre) params['cuatrimestre'] = String(cuatrimestre);
    if (anioCarrera)  params['anioCarrera']  = String(anioCarrera);
    if (tipoExamen)   params['tipoExamen']   = String(tipoExamen);
    params['granularidad'] = granularidad;
    return this.http.get<ReporteEvolucionNotas>(`${this.apiUrl}/rendimiento/evolucion`, { params });
  }

  /** RR-07: Promedios por cátedra. */
  obtenerPromediosCatedra(anio?: number, cursoId?: number): Observable<ReportePromediosCatedra> {
    const params: Record<string, string> = {};
    if (anio)    params['anio']    = String(anio);
    if (cursoId) params['cursoId'] = String(cursoId);
    return this.http.get<ReportePromediosCatedra>(`${this.apiUrl}/rendimiento/catedras`, { params });
  }

  /** Riesgo académico por estudiante (Bajo / Medio / Alto). */
  obtenerRiesgoAcademico(
    anioCohorte?: number,
    carreraId?:   number,
    nivelRiesgo?: string,
  ): Observable<ReporteRiesgoAcademico> {
    const params: Record<string, string> = {};
    if (anioCohorte) params['anioCohorte'] = String(anioCohorte);
    if (carreraId)   params['carreraId']   = String(carreraId);
    if (nivelRiesgo) params['nivelRiesgo'] = nivelRiesgo;
    return this.http.get<ReporteRiesgoAcademico>(`${this.apiUrl}/riesgo-academico`, { params });
  }

  /** RR-01: Tablero ejecutivo institucional — métricas globales para Dirección. */
  obtenerTableroEjecutivo(): Observable<TableroEjecutivo> {
    return this.http.get<TableroEjecutivo>(`${this.apiUrl}/tablero-ejecutivo`);
  }

  /** Retención y deserción agrupada por año de ingreso (cohorte). */
  obtenerRetencionCohorte(carreraId?: number): Observable<ReporteRetencionCohorte> {
    const params: Record<string, string> = {};
    if (carreraId) params['carreraId'] = String(carreraId);
    return this.http.get<ReporteRetencionCohorte>(`${this.apiUrl}/retencion-cohorte`, { params });
  }
}
