import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CursosService, Curso } from '../cursos.service';

@Component({
  selector: 'app-lista-cursos',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './lista-cursos.component.html',
  styleUrl: './lista-cursos.component.scss'
})
export class ListaCursosComponent implements OnInit {
  cursos   = signal<Curso[]>([]);
  cargando = signal(true);
  error    = signal<string | null>(null);

  constructor(private cursosService: CursosService, private router: Router) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.cursosService.listar().subscribe({
      next: data => { this.cursos.set(data); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar los cursos.'); this.cargando.set(false); }
    });
  }

  cerrar(curso: Curso): void {
    // Misma advertencia que en la pantalla del preceptor (mis-cursos): cerrar el acta liquida la
    // cursada de cada alumno y eso no lo deshace "Reactivar" — reactivar solo vuelve el curso a
    // Activo, el historial y el estado de las inscripciones quedan como los dejó el cierre.
    const mensaje =
      `¿Cerrar el acta de la comisión ${curso.comision} (${curso.anioLectivo}° año, ${curso.anio})?\n\n` +
      'Se va a liquidar la cursada de todos los alumnos inscriptos: cada uno queda Regular o ' +
      'Libre según su asistencia. Esta acción no se puede deshacer.';
    if (!confirm(mensaje)) return;

    this.cursosService.cerrar(curso.id).subscribe({
      next: () => this.cargar(),
      error: err => this.error.set(err?.error?.detail ?? err?.error?.title ?? 'Error al cerrar el acta.')
    });
  }

  // "Reabrir curso", no "Reactivar": esta acción solo devuelve el curso al estado Activo (vuelve
  // a aceptar inscripciones). NO revierte la liquidación del acta — el HistorialAcademico y el
  // estado de las inscripciones que generó el cierre quedan como están. Ver ReactivarCursoUseCase.
  reactivar(curso: Curso): void {
    const mensaje =
      `¿Reabrir la comisión ${curso.comision} (${curso.anioLectivo}° año, ${curso.anio})?\n\n` +
      'El curso vuelve a aceptar inscripciones. La liquidación del acta ya realizada ' +
      '(Regular/Libre de cada alumno) no se revierte.';
    if (!confirm(mensaje)) return;

    this.cursosService.reactivar(curso.id).subscribe({
      next: () => this.cargar(),
      error: err => this.error.set(err?.error?.detail ?? err?.error?.title ?? 'Error al reabrir el curso.')
    });
  }

  irACrear(): void          { this.router.navigate(['/cursos/nuevo']); }
  irAEditar(id: number): void { this.router.navigate(['/cursos', id, 'editar']); }
  irAlDashboard(): void     { this.router.navigate(['/dashboard']); }
}
