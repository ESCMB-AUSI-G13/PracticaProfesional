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
  private readonly carreras$ = this.http.get<Carrera[]>(this.apiUrl).pipe(shareReplay(1));

  constructor(private http: HttpClient) {}

  listar(): Observable<Carrera[]> {
    return this.carreras$;
  }
}
