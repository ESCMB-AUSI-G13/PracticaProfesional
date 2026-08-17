import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PreceptoresService, Preceptor } from '../preceptores.service';

@Component({
  selector: 'app-lista-preceptores',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './lista-preceptores.component.html',
  styleUrl: './lista-preceptores.component.scss'
})
export class ListaPreceptoresComponent implements OnInit {
  private todosLosPreceptores = signal<Preceptor[]>([]);
  cargando = signal(true);
  error = signal<string | null>(null);
  busqueda = signal('');

  preceptores = computed(() => {
    const todos = this.todosLosPreceptores();
    const texto = this.busqueda().toLowerCase().trim();
    if (!texto) return todos;

    return todos.filter(p =>
      p.nombre.toLowerCase().includes(texto)   ||
      p.apellido.toLowerCase().includes(texto) ||
      p.legajo.toLowerCase().includes(texto)   ||
      p.email.toLowerCase().includes(texto)    ||
      p.dni.toLowerCase().includes(texto)      ||
      p.turno.toLowerCase().includes(texto)
    );
  });

  constructor(
    private preceptoresService: PreceptoresService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargarPreceptores();
  }

  cargarPreceptores(): void {
    this.cargando.set(true);
    this.error.set(null);

    this.preceptoresService.listar().subscribe({
      next: (data) => {
        this.todosLosPreceptores.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('Error al cargar los preceptores.');
        this.cargando.set(false);
      }
    });
  }

  onBusqueda(texto: string): void { this.busqueda.set(texto); }

  hayPreceptoresEnBD(): boolean { return this.todosLosPreceptores().length > 0; }

  desactivar(usuarioId: number): void {
    if (!confirm('¿Desactivar este preceptor?')) return;
    this.preceptoresService.desactivar(usuarioId).subscribe({
      next: () => this.cargarPreceptores(),
      error: () => this.error.set('Error al desactivar el preceptor.')
    });
  }

  reactivar(usuarioId: number): void {
    if (!confirm('¿Reactivar este preceptor?')) return;
    this.preceptoresService.reactivar(usuarioId).subscribe({
      next: () => this.cargarPreceptores(),
      error: () => this.error.set('Error al reactivar el preceptor.')
    });
  }

  irACrear(): void {
    this.router.navigate(['/preceptores/nuevo']);
  }

  irAEditar(usuarioId: number): void {
    this.router.navigate(['/preceptores', usuarioId, 'editar']);
  }

  irAlDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}
