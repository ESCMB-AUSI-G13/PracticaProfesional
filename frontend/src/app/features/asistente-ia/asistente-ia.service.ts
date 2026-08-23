import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AsistenteRespuesta {
  respuesta: string;
  herramientaUsada: string | null;
  generadoEn: string;
}

@Injectable({ providedIn: 'root' })
export class AsistenteIAService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/asistente-ia`;

  preguntar(pregunta: string): Observable<AsistenteRespuesta> {
    return this.http.post<AsistenteRespuesta>(`${this.apiUrl}/preguntar`, { pregunta });
  }
}
