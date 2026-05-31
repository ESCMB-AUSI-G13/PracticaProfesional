import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PadronAlumnoDto {
  dni: string;
  fechaCarga: string;
}

export interface ImportarPadronResultDto {
  total: number;
  cargados: number;
  fallidos: number;
  errores: { dni: string; motivo: string }[];
}

@Injectable({ providedIn: 'root' })
export class PadronService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/padron`;

  listar(): Observable<PadronAlumnoDto[]> {
    return this.http.get<PadronAlumnoDto[]>(this.apiUrl);
  }

  agregarDni(dni: string): Observable<void> {
    return this.http.post<void>(this.apiUrl, { dni });
  }

  importarCsv(archivo: File): Observable<ImportarPadronResultDto> {
    const formData = new FormData();
    formData.append('archivo', archivo);
    return this.http.post<ImportarPadronResultDto>(`${this.apiUrl}/importar`, formData);
  }

  eliminar(dni: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${dni}`);
  }
}
