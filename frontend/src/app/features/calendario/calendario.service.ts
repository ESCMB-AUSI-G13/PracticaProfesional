import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EventoCalendario {
  id:           number;
  nombreEvento: string;
  comision:     string;
  fechaInicio:  string;
  fechaFin:     string;
  tipoEvento:   string;
}

export interface CrearEventoRequest {
  nombreEvento: string;
  comision:     string;
  fechaInicio:  string;
  fechaFin:     string;
  tipoEvento:   number;
}

export const TIPOS_EVENTO: { valor: number; etiqueta: string }[] = [
  { valor: 1, etiqueta: 'Inicio de clases' },
  { valor: 2, etiqueta: 'Fin de clases' },
  { valor: 3, etiqueta: 'Período de exámenes' },
  { valor: 4, etiqueta: 'Inscripción a materias' },
  { valor: 5, etiqueta: 'Inscripción a exámenes' },
  { valor: 6, etiqueta: 'Fecha límite carga de notas' },
  { valor: 7, etiqueta: 'Feriado' },
  { valor: 8, etiqueta: 'Otro' },
];

export function etiquetaTipo(tipo: string): string {
  const map: Record<string, string> = {
    InicioClases:          'Inicio de clases',
    FinClases:             'Fin de clases',
    PeriodoExamen:         'Período de exámenes',
    InscripcionMateria:    'Inscripción a materias',
    InscripcionExamen:     'Inscripción a exámenes',
    FechaLimiteCargaNotas: 'Límite carga de notas',
    Feriado:               'Feriado',
    Otro:                  'Otro',
  };
  return map[tipo] ?? tipo;
}

@Injectable({ providedIn: 'root' })
export class CalendarioService {
  private readonly apiUrl = 'http://localhost:5000/api/calendario';

  constructor(private http: HttpClient) {}

  listar(anio?: number): Observable<EventoCalendario[]> {
    const params = anio ? `?anio=${anio}` : '';
    return this.http.get<EventoCalendario[]>(`${this.apiUrl}${params}`);
  }

  crear(dto: CrearEventoRequest): Observable<EventoCalendario> {
    return this.http.post<EventoCalendario>(this.apiUrl, dto);
  }

  modificar(id: number, dto: CrearEventoRequest): Observable<EventoCalendario> {
    return this.http.put<EventoCalendario>(`${this.apiUrl}/${id}`, dto);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
