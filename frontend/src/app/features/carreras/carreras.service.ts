import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';

export interface Carrera {
  id:         number;
  nombre:     string;
  resolucion: string;
}

@Injectable({ providedIn: 'root' })
export class CarrerasService {
  private readonly apiUrl = `${environment.apiUrl}/carreras`;
  // Patrón lazy documentado en CLAUDE.md: si en el futuro se agrega un método
  // crear/modificar/eliminar, tiene que invalidar el caché con `this.cache$ = null` después de
  // cada mutación exitosa. Con el campo `readonly` de antes eso ni compilaba (ver CHECKLIST.md,
  // Tier 7 #34) — no hay ninguna mutación implementada todavía, pero queda listo para cuando
  // haga falta.
  private cache$: Observable<Carrera[]> | null = null;

  constructor(private http: HttpClient) {}

  listar(): Observable<Carrera[]> {
    if (!this.cache$) {
      this.cache$ = this.http.get<Carrera[]>(this.apiUrl).pipe(shareReplay(1));
    }
    return this.cache$;
  }
}
