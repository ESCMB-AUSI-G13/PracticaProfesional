import { Component, OnInit, signal, computed } from '@angular/core';
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
  private todosLosDocentes = signal<Docente[]>([]);
  cargando = signal(true);
  error = signal<string | null>(null);
  busqueda = signal('');

  docentes = computed(() => {
    const todos = this.todosLosDocentes();
    const texto = this.busqueda().toLowerCase().trim();
    if (!texto) return todos;

    return todos.filter(d =>
      d.nombre.toLowerCase().includes(texto)   ||
      d.apellido.toLowerCase().includes(texto) ||
      d.legajo.toLowerCase().includes(texto)   ||
      d.email.toLowerCase().includes(texto)    ||
      d.dni.toLowerCase().includes(texto)      ||
      d.categoria.toLowerCase().includes(texto)
    );
  });

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
        this.todosLosDocentes.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('Error al cargar los docentes.');
        this.cargando.set(false);
      }
    });
  }

  onBusqueda(texto: string): void { this.busqueda.set(texto); }

  hayDocentesEnBD(): boolean { return this.todosLosDocentes().length > 0; }

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
