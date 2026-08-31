import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface Parcial {
  fechaExamen: string;
  nota:        number | null;
  estado:      string;
}

export interface HistorialMateria {
  materiaId:     number;
  materiaCodigo: string;
  materiaNombre: string;
  parciales:     Parcial[];
  notaFinal:     number | null;
  estadoFinal:   string | null;
  condicion:     string | null;
  anio:          number | null;
}

export interface MiHistorial {
  materias:        HistorialMateria[];
  promedioGeneral: number | null;
}

@Injectable({ providedIn: 'root' })
export class MiHistorialService {
  private readonly apiUrl = `${environment.apiUrl}/historial-academico`;

  constructor(private http: HttpClient) {}

  obtenerMiHistorial(): Observable<MiHistorial> {
    return this.http.get<MiHistorial>(`${this.apiUrl}/mio`);
  }
}
