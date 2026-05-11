import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ExamenesService, Examen, CrearExamenRequest, TIPOS_EXAMEN } from '../examenes.service';
import { MateriasService, Materia } from '../../materias/materias.service';

@Component({
  selector: 'app-lista-examenes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './lista-examenes.component.html',
  styleUrl: './lista-examenes.component.scss'
})
export class ListaExamenesComponent implements OnInit {
  _todos = signal<Examen[]>([]);
  materias       = signal<Materia[]>([]);
  tiposExamen    = TIPOS_EXAMEN;

  cargando    = signal(true);
  mostrarForm = signal(false);
  guardando   = signal(false);
  error       = signal<string | null>(null);

  // Filtros
  filtroBusqueda  = signal('');
  filtroTipo      = signal('');
  filtroFechaDesde = signal('');
  filtroFechaHasta = signal('');

  examenes = computed(() => {
    let lista     = this._todos();
    const texto   = this.filtroBusqueda().toLowerCase().trim();
    const tipo    = this.filtroTipo();
    const desde   = this.filtroFechaDesde();
    const hasta   = this.filtroFechaHasta();

    if (texto)  lista = lista.filter(e => e.materiaNombre.toLowerCase().includes(texto));
    if (tipo)   lista = lista.filter(e => e.tipoExamen === tipo);
    if (desde)  lista = lista.filter(e => e.fechaExamen >= desde);
    if (hasta)  lista = lista.filter(e => e.fechaExamen <= hasta);

    return lista;
  });

  hayFiltros = computed(() =>
    !!this.filtroBusqueda() || !!this.filtroTipo() || !!this.filtroFechaDesde() || !!this.filtroFechaHasta()
  );

  nuevoMateriaId  = signal<number | null>(null);
  nuevoFecha      = signal('');
  nuevoHorario    = signal('');
  nuevoCupo       = signal<number>(30);
  nuevoTipo       = signal('Parcial');

  constructor(
    private service: ExamenesService,
    private materiasService: MateriasService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.materiasService.listar().subscribe({
      next: m => this.materias.set(m),
      error: () => {}
    });
    this.service.listar().subscribe({
      next: e => { this._todos.set(e); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar exámenes.'); this.cargando.set(false); }
    });
  }

  guardar(): void {
    if (!this.nuevoMateriaId() || !this.nuevoFecha() || !this.nuevoHorario() || !this.nuevoTipo()) {
      this.error.set('Completá todos los campos requeridos.');
      return;
    }
    this.guardando.set(true);
    this.error.set(null);
    const dto: CrearExamenRequest = {
      materiaId:   this.nuevoMateriaId()!,
      fechaExamen: this.nuevoFecha(),
      horario:     this.nuevoHorario(),
      cupo:        this.nuevoCupo(),
      tipoExamen:  this.nuevoTipo()
    };
    this.service.crear(dto).subscribe({
      next: ex => {
        this._todos.update(list => [ex, ...list]);
        this.limpiarForm();
        this.guardando.set(false);
      },
      error: (e) => { this.error.set(e.error?.mensaje ?? 'Error al crear el examen.'); this.guardando.set(false); }
    });
  }

  limpiarForm(): void {
    this.nuevoMateriaId.set(null);
    this.nuevoFecha.set('');
    this.nuevoHorario.set('');
    this.nuevoCupo.set(30);
    this.nuevoTipo.set('Parcial');
    this.mostrarForm.set(false);
  }

  eliminar(ex: Examen): void {
    const confirmado = confirm(
      `¿Eliminar el examen de "${ex.materiaNombre}" (${ex.tipoExamen} — ${new Date(ex.fechaExamen).toLocaleDateString('es-AR')})?\n\nSe eliminarán también todas las inscripciones asociadas.`
    );
    if (!confirmado) return;

    this.service.eliminar(ex.id).subscribe({
      next: () => this._todos.update(list => list.filter(e => e.id !== ex.id)),
      error: (e) => this.error.set(e.error?.detail ?? 'Error al eliminar el examen.')
    });
  }

  limpiarFiltros(): void {
    this.filtroBusqueda.set('');
    this.filtroTipo.set('');
    this.filtroFechaDesde.set('');
    this.filtroFechaHasta.set('');
  }

  irAlDashboard(): void { this.router.navigate(['/dashboard']); }
}
