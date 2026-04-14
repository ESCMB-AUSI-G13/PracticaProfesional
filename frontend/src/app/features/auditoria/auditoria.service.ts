import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AuditoriaLogDto {
  id: number;
  entidadTipo: string;
  entidadId: string;
  accion: string;
  ejecutorId: number | null;
  ejecutorEmail: string;
  valorAnterior: string | null;
  valorNuevo: string | null;
  timestamp: string;
}

export interface PaginadoDto<T> {
  items: T[];
  totalRegistros: number;
  pagina: number;
  tamanoPagina: number;
  totalPaginas: number;
}

export interface AuditoriaFiltros {
  entidadTipo?: string;
  accion?: string;
  ejecutorEmail?: string;
  fechaDesde?: string;
  fechaHasta?: string;
  pagina?: number;
  tamanoPagina?: number;
}

@Injectable({ providedIn: 'root' })
export class AuditoriaService {
  private readonly apiUrl = 'http://localhost:5000/api/auditoria';

  constructor(private http: HttpClient) {}

  listarLogs(filtros: AuditoriaFiltros): Observable<PaginadoDto<AuditoriaLogDto>> {
    let params = new HttpParams();

    if (filtros.entidadTipo) params = params.set('entidadTipo', filtros.entidadTipo);
    if (filtros.accion)      params = params.set('accion', filtros.accion);
    if (filtros.ejecutorEmail) params = params.set('ejecutorEmail', filtros.ejecutorEmail);
    if (filtros.fechaDesde)  params = params.set('fechaDesde', filtros.fechaDesde);
    if (filtros.fechaHasta)  params = params.set('fechaHasta', filtros.fechaHasta);

    params = params
      .set('pagina', String(filtros.pagina ?? 1))
      .set('tamanoPagina', String(filtros.tamanoPagina ?? 50));

    return this.http.get<PaginadoDto<AuditoriaLogDto>>(`${this.apiUrl}/logs`, { params });
  }
}
