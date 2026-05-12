import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface EspacioAsistencia {
  espacioCurricularId: number;
  cursoId:             number;
  materiaId:           number;
  materiaNombre:       string;
  anioLectivo:         number;
  comision:            string;
}

export interface AlumnoParaAsistencia {
  estudianteId:    number;
  nombreCompleto:  string;
  legajo:          string;
}

export interface AusenciaItem {
  estudianteId: number;
  tipoAusencia: 'Justificada' | 'Injustificada';
  motivo?:      string;
}

export interface RegistrarAsistenciasCommand {
  espacioCurricularId: number;
  fecha:               string;
  ausentes:            AusenciaItem[];
}

export interface DetalleAusencia {
  nombreCompleto: string;
  legajo:         string;
  tipoAusencia:   string;
  motivo:         string | null;
}

export interface ResumenAsistencias {
  fecha:                  string;
  materiaNombre:          string;
  cursoComision:          string;
  anioLectivo:            number;
  totalAlumnos:           number;
  presentes:              number;
  ausentesInjustificados: number;
  ausentesJustificados:   number;
  ausentes:               DetalleAusencia[];
}

export interface AsistenciaDetalle {
  asistenciaId:   number;
  estudianteId:   number;
  nombreCompleto: string;
  legajo:         string;
  estado:         string;
  motivo:         string | null;
}

export interface RegistroDelDia {
  espacioCurricularId: number;
  cursoId:             number;
  materiaId:           number;
  materiaNombre:       string;
  anioLectivo:         number;
  comision:            string;
  fecha:               string;
  alumnos:             AsistenciaDetalle[];
}

export interface CambioAsistenciaItem {
  asistenciaId: number;
  nuevoEstado:  string;
  motivo?:      string;
}

export interface RectificarAsistenciasCommand {
  espacioCurricularId: number;
  fecha:               string;
  cambios:             CambioAsistenciaItem[];
}

@Injectable({ providedIn: 'root' })
export class AsistenciasService {
  private readonly base = `${environment.apiUrl}/asistencias`;

  constructor(private http: HttpClient) {}

  obtenerMisEspacios(): Observable<EspacioAsistencia[]> {
    return this.http.get<EspacioAsistencia[]>(`${this.base}/mis-espacios`);
  }

  obtenerAlumnosPorEspacio(espacioCurricularId: number): Observable<AlumnoParaAsistencia[]> {
    return this.http.get<AlumnoParaAsistencia[]>(`${this.base}/espacios/${espacioCurricularId}/alumnos`);
  }

  registrarAsistencias(command: RegistrarAsistenciasCommand): Observable<ResumenAsistencias> {
    return this.http.post<ResumenAsistencias>(this.base, command);
  }

  obtenerRegistroDelDia(espacioCurricularId: number, fecha: string): Observable<RegistroDelDia> {
    return this.http.get<RegistroDelDia>(`${this.base}/espacios/${espacioCurricularId}/fecha/${fecha}`);
  }

  rectificarAsistencias(command: RectificarAsistenciasCommand): Observable<void> {
    return this.http.put<void>(`${this.base}/rectificar`, command);
  }
}
