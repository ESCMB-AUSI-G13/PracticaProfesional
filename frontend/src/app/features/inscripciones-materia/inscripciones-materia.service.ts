import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface InscripcionMateria {
  id:               number;
  estudianteId:     number;
  estudianteNombre: string;
  materiaId:        number;
  materiaCodigo:    string;
  materiaNombre:    string;
  cursoId:          number;
  cursoAnio:        number;
  cursoComision:    string;
  estado:           string;
  fechaInscripcion: string;
}

export interface CrearInscripcionMateriaRequest {
  estudianteId: number;
  materiaId:    number;
  cursoId:      number;
}

export interface ComprobanteInscripcionMateria {
  id:                      number;
  estudianteNombreCompleto: string;
  estudianteDni:           string;
  estudianteLegajo:        string;
  materiaCodigo:           string;
  materiaNombre:           string;
  materiaPlan:             string;
  cursoAnioLectivo:        number;
  cursoComision:           string;
  estado:                  string;
  fechaInscripcion:        string;
  fechaEmision:            string;
}

@Injectable({ providedIn: 'root' })
export class InscripcionesMateriaService {
  private readonly apiUrl = 'http://localhost:5000/api/inscripciones/materias';

  constructor(private http: HttpClient) {}

  listar(): Observable<InscripcionMateria[]> {
    return this.http.get<InscripcionMateria[]>(this.apiUrl);
  }

  inscribir(dto: CrearInscripcionMateriaRequest): Observable<any> {
    return this.http.post(this.apiUrl, dto);
  }

  obtenerComprobante(id: number): Observable<ComprobanteInscripcionMateria> {
    return this.http.get<ComprobanteInscripcionMateria>(`${this.apiUrl}/${id}/comprobante`);
  }
}
