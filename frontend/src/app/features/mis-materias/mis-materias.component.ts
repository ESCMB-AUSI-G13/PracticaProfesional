import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { MisMateriasService, InscripcionMateria, ComprobanteInscripcionMateria } from './mis-materias.service';
import { MateriasService, Materia } from '../materias/materias.service';
import { CursosService, Curso } from '../cursos/cursos.service';
import { EncuestasService, EncuestaDto } from '../encuestas/encuestas.service';
import { ModalEncuestaComponent } from '../encuestas/modal-encuesta/modal-encuesta.component';
import { CargandoComponent } from '../../shared/cargando/cargando.component';

@Component({
  selector: 'app-mis-materias',
  standalone: true,
  imports: [CommonModule, FormsModule, ModalEncuestaComponent, CargandoComponent],
  templateUrl: './mis-materias.component.html',
  styleUrl: './mis-materias.component.scss'
})
export class MisMateriasComponent implements OnInit {
  inscripciones = signal<InscripcionMateria[]>([]);
  materias      = signal<Materia[]>([]);
  cursos        = signal<Curso[]>([]);

  cargando       = signal(true);
  cargandoCursos = signal(false);
  mostrarForm    = signal(false);
  guardando      = signal(false);
  error          = signal<string | null>(null);

  comprobante         = signal<ComprobanteInscripcionMateria | null>(null);
  cargandoComprobante = signal(false);
  encuestaPendiente   = signal<EncuestaDto | null>(null);
  dandoDeBajaId       = signal<number | null>(null);

  selectedMateriaId = signal<number | null>(null);
  selectedCursoId   = signal<number | null>(null);

  constructor(
    private service: MisMateriasService,
    private materiasService: MateriasService,
    private cursosService: CursosService,
    private encuestasService: EncuestasService
  ) {}

  ngOnInit(): void {
    let errorInscripciones: string | null = null;
    let errorMaterias: string | null = null;

    forkJoin({
      inscripciones: this.service.listarMisInscripciones().pipe(
        catchError((e: { error?: { detail?: string }; message?: string }) => {
          errorInscripciones = `inscripciones (${e.error?.detail ?? e.message ?? 'desconocido'})`;
          return of([] as InscripcionMateria[]);
        })
      ),
      materias: this.materiasService.listarMiCarrera().pipe(
        catchError((e: { error?: { detail?: string }; message?: string }) => {
          errorMaterias = `materias (${e.error?.detail ?? e.message ?? 'desconocido'})`;
          return of([] as Materia[]);
        })
      )
    }).subscribe({
      next: ({ inscripciones, materias }) => {
        this.inscripciones.set(inscripciones);
        this.materias.set(materias);
        this.cargando.set(false);

        // Si ambas cargas fallan, un solo signal de error no debe perder la primera.
        const errores = [errorInscripciones, errorMaterias].filter((e): e is string => e !== null);
        if (errores.length > 0)
          this.error.set(`Error al cargar ${errores.join(' y ')}.`);
      }
    });
  }

  onMateriaChange(materiaId: number | null): void {
    this.selectedMateriaId.set(materiaId);
    this.selectedCursoId.set(null);
    this.cursos.set([]);

    if (!materiaId) return;

    this.cargandoCursos.set(true);
    this.cursosService.listarPorMateria(materiaId).pipe(
      catchError((e: { error?: { detail?: string }; message?: string }) => {
        this.error.set(`Error al cargar cursos: ${e.error?.detail ?? e.message ?? 'desconocido'}`);
        return of([] as Curso[]);
      })
    ).subscribe(cursos => {
      this.cursos.set(cursos);
      this.cargandoCursos.set(false);
    });
  }

  inscribirse(): void {
    if (this.guardando()) return; // evita disparar varias inscripciones en carrera con clicks repetidos

    if (!this.selectedMateriaId() || !this.selectedCursoId()) {
      this.error.set('Seleccioná una materia y un curso.');
      return;
    }
    this.error.set(null);
    this.guardando.set(true);

    this.encuestasService.obtenerPendiente().subscribe({
      next: (encuesta) => {
        if (encuesta) {
          this.guardando.set(false);
          this.encuestaPendiente.set(encuesta);
        } else {
          this.ejecutarInscripcion();
        }
      },
      // No dejar pasar la inscripción si no se pudo verificar la encuesta obligatoria:
      // el backend igual bloquearía con 428, pero acá el usuario vería un error genérico
      // en vez de que se le muestre la encuesta que tenía que completar.
      error: () => {
        this.guardando.set(false);
        this.error.set('No se pudo verificar si tenés una encuesta pendiente. Reintentá en unos segundos.');
      }
    });
  }

  onEncuestaCompletada(): void {
    this.encuestaPendiente.set(null);
    this.ejecutarInscripcion();
  }

  cerrarEncuestaPendiente(): void {
    this.encuestaPendiente.set(null);
    this.guardando.set(false);
  }

  private ejecutarInscripcion(): void {
    this.guardando.set(true);
    this.service.inscribirse({ materiaId: this.selectedMateriaId()!, cursoId: this.selectedCursoId()! }).subscribe({
      next: (resultado) => {
        this.guardando.set(false);
        this.limpiarForm();
        this.cargandoComprobante.set(true);
        this.service.obtenerComprobante(resultado.id).subscribe({
          next: (c) => { this.comprobante.set(c); this.cargandoComprobante.set(false); },
          error: () => { this.cargandoComprobante.set(false); }
        });
        this.service.listarMisInscripciones().subscribe({
          next: list => this.inscripciones.set(list),
          error: () => this.error.set('La inscripción se registró, pero no se pudo actualizar la lista. Recargá la página para verla.')
        });
      },
      error: (e) => {
        this.error.set(e.error?.detail ?? e.error?.mensaje ?? 'Error al inscribirse.');
        this.guardando.set(false);
      }
    });
  }

  darDeBaja(i: InscripcionMateria): void {
    if (this.dandoDeBajaId() !== null) return;

    const confirmado = confirm(
      `¿Dar de baja tu inscripción en "${i.materiaNombre}"?\n\nEsta acción no se puede deshacer.`
    );
    if (!confirmado) return;

    this.dandoDeBajaId.set(i.id);
    this.service.darDeBaja(i.id).subscribe({
      next: () => {
        this.dandoDeBajaId.set(null);
        this.service.listarMisInscripciones().subscribe({
          next: list => this.inscripciones.set(list),
          error: () => this.error.set('La baja se registró, pero no se pudo actualizar la lista. Recargá la página para verla.')
        });
      },
      error: (e) => {
        this.dandoDeBajaId.set(null);
        this.error.set(e.error?.detail ?? 'Error al dar de baja la inscripción.');
      }
    });
  }

  verComprobante(id: number): void {
    this.cargandoComprobante.set(true);
    this.service.obtenerComprobante(id).subscribe({
      next: (c) => { this.comprobante.set(c); this.cargandoComprobante.set(false); },
      error: () => { this.error.set('Error al obtener el comprobante.'); this.cargandoComprobante.set(false); }
    });
  }

  limpiarForm(): void {
    this.selectedMateriaId.set(null);
    this.selectedCursoId.set(null);
    this.cursos.set([]);
    this.mostrarForm.set(false);
  }

  cerrarComprobante(): void { this.comprobante.set(null); }

  imprimirComprobante(): void { window.print(); }

  estadoLabel(estado: string): string {
    const map: Record<string, string> = {
      'pendiente':   '○ Pendiente',
      'activa':      '● Activa',
      'aprobada':    '✓ Aprobada',
      'desaprobada': '✗ Desaprobada',
      'baja':        '✗ Baja',
    };
    return map[estado.toLowerCase()] ?? estado;
  }
}
