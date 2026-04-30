import { Component, OnInit, signal } from '@angular/core';
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
  inscripciones = signal<InscripcionMateria[]>([]);
  estudiantes   = signal<Estudiante[]>([]);
  materias      = signal<Materia[]>([]);
  cursos        = signal<Curso[]>([]);

  cargando    = signal(true);
  mostrarForm = signal(false);
  guardando   = signal(false);
  error       = signal<string | null>(null);

  comprobante         = signal<ComprobanteInscripcionMateria | null>(null);
  cargandoComprobante = signal(false);

  nuevoEstudianteId = signal<number | null>(null);
  nuevoMateriaId    = signal<number | null>(null);
  nuevoCursoId      = signal<number | null>(null);

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
        this.inscripciones.set(inscripciones);
        this.estudiantes.set(estudiantes.filter(e => e.activo && e.condicion !== 'Egresado' && e.condicion !== 'Desertor'));
        this.materias.set(materias);
        this.cursos.set(cursos.filter(c => c.estado === 'Activo'));
        this.cargando.set(false);
      },
      error: () => { this.error.set('Error al cargar datos.'); this.cargando.set(false); }
    });
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
        this.service.listar().subscribe(list => this.inscripciones.set(list));
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
