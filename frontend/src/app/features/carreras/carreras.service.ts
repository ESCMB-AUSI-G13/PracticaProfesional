import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Carrera {
  id:         number;
  nombre:     string;
  resolucion: string;
}

@Injectable({ providedIn: 'root' })
export class CarrerasService {
  private readonly apiUrl = 'http://localhost:5000/api/carreras';

  constructor(private http: HttpClient) {}

  listar(): Observable<Carrera[]> {
    return this.http.get<Carrera[]>(this.apiUrl);
  }
}
