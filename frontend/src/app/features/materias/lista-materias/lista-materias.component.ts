import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MateriasService, Materia } from '../materias.service';
import { CarrerasService, Carrera } from '../../carreras/carreras.service';

@Component({
  selector: 'app-lista-materias',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './lista-materias.component.html',
  styleUrl: './lista-materias.component.scss'
})
export class ListaMateriasComponent implements OnInit {
  materias = signal<Materia[]>([]);
  carreras = signal<Carrera[]>([]);
  cargando = signal(true);
  error    = signal<string | null>(null);

  filtroCarreraId = signal<number | null>(null);
  filtroAnio      = signal<number | null>(null);
  filtroBusqueda  = signal('');

  materiasFiltradas = computed(() => {
    let lista      = this.materias();
    const carreraId = this.filtroCarreraId();
    const anio      = this.filtroAnio();
    const busqueda  = this.filtroBusqueda().toLowerCase().trim();

    if (carreraId) lista = lista.filter(m => m.carreraId === carreraId);
    if (anio)      lista = lista.filter(m => m.anio === anio);
    if (busqueda)  lista = lista.filter(m => m.nombre.toLowerCase().includes(busqueda));

    return lista;
  });

  aniosDisponibles = computed(() =>
    [...new Set(this.materias().map(m => m.anio))].sort()
  );

  hayFiltros = computed(() =>
    !!this.filtroCarreraId() || !!this.filtroAnio() || !!this.filtroBusqueda()
  );

  constructor(
    private materiasService: MateriasService,
    private carrerasService: CarrerasService,
    private router: Router
  ) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);

    this.carrerasService.listar().subscribe({
      next: data => this.carreras.set(data)
    });

    this.materiasService.listar().subscribe({
      next: data => { this.materias.set(data); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar las materias.'); this.cargando.set(false); }
    });
  }

  limpiarFiltros(): void {
    this.filtroCarreraId.set(null);
    this.filtroAnio.set(null);
    this.filtroBusqueda.set('');
  }

  irACrear(): void          { this.router.navigate(['/materias/nueva']); }
  irAEditar(id: number): void { this.router.navigate(['/materias', id, 'editar']); }
  irAlDashboard(): void     { this.router.navigate(['/dashboard']); }

  eliminar(m: Materia): void {
    const confirmado = confirm(`¿Eliminar la materia "${m.nombre}" (${m.codigo})?\n\nEsta acción no se puede deshacer.`);
    if (!confirmado) return;

    this.materiasService.eliminar(m.id).subscribe({
      next: () => this.materias.update(list => list.filter(x => x.id !== m.id)),
      error: (e) => this.error.set(e.error?.detail ?? 'Error al eliminar la materia.')
    });
  }
}
