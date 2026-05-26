import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { EstudiantesService, Estudiante } from '../estudiantes.service';

@Component({
  selector: 'app-lista-estudiantes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './lista-estudiantes.component.html',
  styleUrl: './lista-estudiantes.component.scss'
})
export class ListaEstudiantesComponent implements OnInit {
  private _todos = signal<Estudiante[]>([]);
  cargando = signal(true);
  error = signal<string | null>(null);

  filtroBusqueda = signal('');
  filtroCarreraId = signal<number | null>(null);
  filtroAnio = signal<number | null>(null);
  filtroCondicion = signal('');

  readonly condiciones = ['Regular', 'Libre', 'Promocional', 'Egresado', 'Desertor'];

  carreras = computed(() =>
    [...new Map(this._todos().map(e => [e.carreraId, { id: e.carreraId, nombre: e.carreraNombre }])).values()]
      .sort((a, b) => a.id - b.id)
  );

  anios = computed(() =>
    [...new Set(this._todos().map(e => e.anio))].sort((a, b) => a - b)
  );

  estudiantes = computed(() => {
    let lista = this._todos();
    const texto = this.filtroBusqueda().toLowerCase().trim();
    const carreraId = this.filtroCarreraId();
    const anio = this.filtroAnio();
    const condicion = this.filtroCondicion();

    if (texto)
      lista = lista.filter(e =>
        e.nombre.toLowerCase().includes(texto) ||
        e.apellido.toLowerCase().includes(texto) ||
        e.legajo.toLowerCase().includes(texto) ||
        e.dni.includes(texto)
      );
    if (carreraId !== null)
      lista = lista.filter(e => e.carreraId === carreraId);
    if (anio !== null)
      lista = lista.filter(e => e.anio === anio);
    if (condicion)
      lista = lista.filter(e => e.condicion === condicion);

    return lista;
  });

  hayFiltrosActivos = computed(() =>
    !!this.filtroBusqueda() ||
    this.filtroCarreraId() !== null ||
    this.filtroAnio() !== null ||
    !!this.filtroCondicion()
  );

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
        this._todos.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('Error al cargar los estudiantes.');
        this.cargando.set(false);
      }
    });
  }

  onBusqueda(valor: string): void     { this.filtroBusqueda.set(valor); }
  onCarrera(valor: string): void      { this.filtroCarreraId.set(valor ? +valor : null); }
  onAnio(valor: string): void         { this.filtroAnio.set(valor ? +valor : null); }
  onCondicion(valor: string): void    { this.filtroCondicion.set(valor); }

  limpiarFiltros(): void {
    this.filtroBusqueda.set('');
    this.filtroCarreraId.set(null);
    this.filtroAnio.set(null);
    this.filtroCondicion.set('');
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

  eliminar(usuarioId: number, nombre: string, apellido: string): void {
    if (!confirm(`¿Eliminar permanentemente a ${apellido}, ${nombre}?\n\nEsta acción no se puede deshacer y borrará todos sus datos (inscripciones, asistencias, historial).`)) return;
    this.estudiantesService.eliminar(usuarioId).subscribe({
      next: () => this.cargarEstudiantes(),
      error: () => this.error.set('Error al eliminar el estudiante.')
    });
  }

  irACrear(): void   { this.router.navigate(['/estudiantes/nuevo']); }
  irAEditar(id: number): void { this.router.navigate(['/estudiantes', id, 'editar']); }
}
