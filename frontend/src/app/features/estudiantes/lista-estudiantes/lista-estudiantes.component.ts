import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { EstudiantesService, Estudiante } from '../estudiantes.service';

@Component({
  selector: 'app-lista-estudiantes',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './lista-estudiantes.component.html',
  styleUrl: './lista-estudiantes.component.scss'
})
export class ListaEstudiantesComponent implements OnInit {
  estudiantes = signal<Estudiante[]>([]);
  cargando = signal(true);
  error = signal<string | null>(null);

  constructor(
    private estudiantesService: EstudiantesService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargarEstudiantes();
  }

  cargarEstudiantes(): void {
    this.cargando.set(true);
    this.error.set(null);

    this.estudiantesService.listar().subscribe({
      next: (data) => {
        this.estudiantes.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('Error al cargar los estudiantes.');
        this.cargando.set(false);
      }
    });
  }

  desactivar(usuarioId: number): void {
    if (!confirm('¿Desactivar este estudiante?')) return;
    this.estudiantesService.desactivar(usuarioId).subscribe({
      next: () => this.cargarEstudiantes(),
      error: () => this.error.set('Error al desactivar el estudiante.')
    });
  }

  reactivar(usuarioId: number): void {
    if (!confirm('¿Reactivar este estudiante?')) return;
    this.estudiantesService.reactivar(usuarioId).subscribe({
      next: () => this.cargarEstudiantes(),
      error: () => this.error.set('Error al reactivar el estudiante.')
    });
  }

  irACrear(): void {
    this.router.navigate(['/estudiantes/nuevo']);
  }

  irAEditar(usuarioId: number): void {
    this.router.navigate(['/estudiantes', usuarioId, 'editar']);
  }

  irAlDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}
