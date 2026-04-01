import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DocentesService, Docente } from '../docentes.service';

@Component({
  selector: 'app-lista-docentes',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './lista-docentes.component.html',
  styleUrl: './lista-docentes.component.scss'
})
export class ListaDocentesComponent implements OnInit {
  docentes = signal<Docente[]>([]);
  cargando = signal(true);
  error = signal<string | null>(null);

  constructor(
    private docentesService: DocentesService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargarDocentes();
  }

  cargarDocentes(): void {
    this.cargando.set(true);
    this.error.set(null);

    this.docentesService.listar().subscribe({
      next: (data) => {
        this.docentes.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('Error al cargar los docentes.');
        this.cargando.set(false);
      }
    });
  }

  desactivar(usuarioId: number): void {
    if (!confirm('¿Desactivar este docente?')) return;
    this.docentesService.desactivar(usuarioId).subscribe({
      next: () => this.cargarDocentes(),
      error: () => this.error.set('Error al desactivar el docente.')
    });
  }

  reactivar(usuarioId: number): void {
    if (!confirm('¿Reactivar este docente?')) return;
    this.docentesService.reactivar(usuarioId).subscribe({
      next: () => this.cargarDocentes(),
      error: () => this.error.set('Error al reactivar el docente.')
    });
  }

  irACrear(): void {
    this.router.navigate(['/docentes/nuevo']);
  }

  irAEditar(usuarioId: number): void {
    this.router.navigate(['/docentes', usuarioId, 'editar']);
  }

  irAlDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}
