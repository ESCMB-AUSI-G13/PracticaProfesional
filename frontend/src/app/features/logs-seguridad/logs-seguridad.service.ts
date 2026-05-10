import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { PaginadoDto } from '../auditoria/auditoria.service';

export interface LogSeguridadDto {
  id: number;
  email: string;
  exitoso: boolean;
  motivoFallo: string | null;
  ipOrigen: string;
  userAgent: string;
  timestamp: string;
}

export interface LogSeguridadFiltros {
  email?: string;
  soloFallidos?: boolean | null;
  fechaDesde?: string;
  fechaHasta?: string;
  pagina?: number;
  tamanoPagina?: number;
}

@Injectable({ providedIn: 'root' })
export class LogsSeguridadService {
  private readonly apiUrl = `${environment.apiUrl}/logs-seguridad`;

  constructor(private http: HttpClient) {}

  listar(filtros: LogSeguridadFiltros): Observable<PaginadoDto<LogSeguridadDto>> {
    let params = new HttpParams()
      .set('pagina',       String(filtros.pagina       ?? 1))
      .set('tamanoPagina', String(filtros.tamanoPagina ?? 50));

    if (filtros.email)                          params = params.set('email',       filtros.email);
    if (filtros.soloFallidos != null)           params = params.set('soloFallidos', String(filtros.soloFallidos));
    if (filtros.fechaDesde)                     params = params.set('fechaDesde',  filtros.fechaDesde);
    if (filtros.fechaHasta)                     params = params.set('fechaHasta',  filtros.fechaHasta);

    return this.http.get<PaginadoDto<LogSeguridadDto>>(this.apiUrl, { params });
  }
}
