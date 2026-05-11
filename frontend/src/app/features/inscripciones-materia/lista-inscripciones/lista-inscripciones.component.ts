import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { InscripcionesMateriaService, InscripcionMateria, CrearInscripcionMateriaRequest, ComprobanteInscripcionMateria } from '../inscripciones-materia.service';
import { EstudiantesService, Estudiante } from '../../estudiantes/estudiantes.service';
import { MateriasService, Materia } from '../../materias/materias.service';
import { CursosService, Curso } from '../../cursos/cursos.service';

@Component({
  selector: 'app-lista-inscripciones',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './lista-inscripciones.component.html',
  styleUrl: './lista-inscripciones.component.scss'
})
export class ListaInscripcionesComponent implements OnInit {
  private _todas = signal<InscripcionMateria[]>([]);
  estudiantes    = signal<Estudiante[]>([]);
  materias       = signal<Materia[]>([]);
  cursos         = signal<Curso[]>([]);

  cargando    = signal(true);
  mostrarForm = signal(false);
  guardando   = signal(false);
  error       = signal<string | null>(null);

  comprobante         = signal<ComprobanteInscripcionMateria | null>(null);
  cargandoComprobante = signal(false);

  nuevoEstudianteId = signal<number | null>(null);
  nuevoMateriaId    = signal<number | null>(null);
  nuevoCursoId      = signal<number | null>(null);

  // ── Filtros ────────────────────────────────────────────────────────────────
  filtroBusqueda  = signal('');
  filtroCarreraId = signal<number | null>(null);
  filtroAnio      = signal<number | null>(null);
  filtroComision  = signal('');

  carreras = computed(() =>
    [...new Map(this._todas().map(i => [i.carreraId, { id: i.carreraId, nombre: i.carreraNombre }])).values()]
      .sort((a, b) => a.id - b.id)
  );

  anios = computed(() =>
    [...new Set(this._todas().map(i => i.cursoAnioLectivo))].sort((a, b) => a - b)
  );

  comisiones = computed(() =>
    [...new Set(this._todas().map(i => i.cursoComision))].sort()
  );

  inscripciones = computed(() => {
    let lista = this._todas();
    const texto = this.filtroBusqueda().toLowerCase().trim();
    const carreraId = this.filtroCarreraId();
    const anio = this.filtroAnio();
    const comision = this.filtroComision();

    if (texto)
      lista = lista.filter(i =>
        i.estudianteNombre.toLowerCase().includes(texto) ||
        i.estudianteLegajo.toLowerCase().includes(texto)
      );
    if (carreraId !== null)
      lista = lista.filter(i => i.carreraId === carreraId);
    if (anio !== null)
      lista = lista.filter(i => i.cursoAnioLectivo === anio);
    if (comision)
      lista = lista.filter(i => i.cursoComision === comision);

    return lista;
  });

  hayFiltrosActivos = computed(() =>
    !!this.filtroBusqueda() ||
    this.filtroCarreraId() !== null ||
    this.filtroAnio() !== null ||
    !!this.filtroComision()
  );

  constructor(
    private service: InscripcionesMateriaService,
    private estudiantesService: EstudiantesService,
    private materiasService: MateriasService,
    private cursosService: CursosService,
    private router: Router
  ) {}

  ngOnInit(): void {
    forkJoin({
      inscripciones: this.service.listar(),
      estudiantes:   this.estudiantesService.listar(),
      materias:      this.materiasService.listar(),
      cursos:        this.cursosService.listar()
    }).subscribe({
      next: ({ inscripciones, estudiantes, materias, cursos }) => {
        this._todas.set(inscripciones);
        this.estudiantes.set(estudiantes.filter(e => e.activo && e.condicion !== 'Egresado' && e.condicion !== 'Desertor'));
        this.materias.set(materias);
        this.cursos.set(cursos.filter(c => c.estado === 'Activo'));
        this.cargando.set(false);
      },
      error: () => { this.error.set('Error al cargar datos.'); this.cargando.set(false); }
    });
  }

  onBusqueda(valor: string): void  { this.filtroBusqueda.set(valor); }
  onCarrera(valor: string): void   { this.filtroCarreraId.set(valor ? +valor : null); }
  onAnio(valor: string): void      { this.filtroAnio.set(valor ? +valor : null); }
  onComision(valor: string): void  { this.filtroComision.set(valor); }

  limpiarFiltros(): void {
    this.filtroBusqueda.set('');
    this.filtroCarreraId.set(null);
    this.filtroAnio.set(null);
    this.filtroComision.set('');
  }

  guardar(): void {
    if (!this.nuevoEstudianteId() || !this.nuevoMateriaId() || !this.nuevoCursoId()) {
      this.error.set('Seleccioná estudiante, materia y curso.');
      return;
    }
    this.guardando.set(true);
    this.error.set(null);
    const dto: CrearInscripcionMateriaRequest = {
      estudianteId: this.nuevoEstudianteId()!,
      materiaId:    this.nuevoMateriaId()!,
      cursoId:      this.nuevoCursoId()!
    };
    this.service.inscribir(dto).subscribe({
      next: () => {
        this.service.listar().subscribe(list => this._todas.set(list));
        this.limpiarForm();
        this.guardando.set(false);
      },
      error: (e) => { this.error.set(e.error?.mensaje ?? 'Error al crear la inscripción.'); this.guardando.set(false); }
    });
  }

  limpiarForm(): void {
    this.nuevoEstudianteId.set(null);
    this.nuevoMateriaId.set(null);
    this.nuevoCursoId.set(null);
    this.mostrarForm.set(false);
  }

  irAlDashboard(): void { this.router.navigate(['/dashboard']); }

  estadoLabel(estado: string): string {
    const map: Record<string, string> = {
      'activa':      '● Activa',
      'aprobada':    '✓ Aprobada',
      'desaprobada': '✗ Desaprobada',
      'baja':        '✗ Baja',
      'pendiente':   '— Pendiente',
    };
    return map[estado.toLowerCase()] ?? estado;
  }

  verComprobante(id: number): void {
    this.cargandoComprobante.set(true);
    this.service.obtenerComprobante(id).subscribe({
      next: (c) => { this.comprobante.set(c); this.cargandoComprobante.set(false); },
      error: () => { this.error.set('Error al obtener el comprobante.'); this.cargandoComprobante.set(false); }
    });
  }

  cerrarComprobante(): void { this.comprobante.set(null); }

  imprimirComprobante(): void { window.print(); }
}
