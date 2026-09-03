import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CursosService, Curso } from '../cursos.service';

/**
 * CU-33: pantalla del preceptor para ver los cursos que tiene a cargo y cerrar sus actas.
 * Es solo lectura + cierre: crear, editar y reactivar cursos siguen siendo de Dirección.
 */
@Component({
  selector: 'app-mis-cursos',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mis-cursos.component.html',
  styleUrl: './mis-cursos.component.scss'
})
export class MisCursosComponent implements OnInit {
  cursos      = signal<Curso[]>([]);
  cargando    = signal(true);
  error       = signal<string | null>(null);
  accionError = signal<string | null>(null);
  cerrandoId  = signal<number | null>(null);

  constructor(private cursosService: CursosService) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.cursosService.listarMisCursos().subscribe({
      next: data => { this.cursos.set(data); this.cargando.set(false); },
      error: err => {
        this.error.set(err?.error?.detail ?? err?.error?.title ?? 'Error al cargar tus cursos.');
        this.cargando.set(false);
      }
    });
  }

  cerrar(curso: Curso): void {
    const mensaje =
      `¿Cerrar el acta de la comisión ${curso.comision} (${curso.anioLectivo}° año, ${curso.anio})?\n\n` +
      'Se va a liquidar la cursada de todos los alumnos inscriptos: cada uno queda Regular o ' +
      'Libre según su asistencia. Esta acción no se puede deshacer.';
    if (!confirm(mensaje)) return;

    this.accionError.set(null);
    this.cerrandoId.set(curso.id);
    this.cursosService.cerrar(curso.id).subscribe({
      next: () => { this.cerrandoId.set(null); this.cargar(); },
      error: err => {
        this.cerrandoId.set(null);
        this.accionError.set(err?.error?.detail ?? err?.error?.title ?? 'Error al cerrar el acta.');
      }
    });
  }
}
