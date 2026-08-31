import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MisExamenesService, ExamenFinalDisponible, ComprobanteInscripcionExamen } from './mis-examenes.service';
import { EncuestasService, EncuestaDto } from '../encuestas/encuestas.service';
import { ModalEncuestaComponent } from '../encuestas/modal-encuesta/modal-encuesta.component';
import { CargandoComponent } from '../../shared/cargando/cargando.component';

@Component({
  selector: 'app-mis-examenes',
  standalone: true,
  imports: [CommonModule, ModalEncuestaComponent, CargandoComponent],
  templateUrl: './mis-examenes.component.html',
  styleUrl: './mis-examenes.component.scss'
})
export class MisExamenesComponent implements OnInit {
  examenes     = signal<ExamenFinalDisponible[]>([]);
  cargando     = signal(true);
  error        = signal<string | null>(null);
  inscribiendo = signal<number | null>(null);
  comprobante  = signal<ComprobanteInscripcionExamen | null>(null);
  encuestaPendiente = signal<EncuestaDto | null>(null);
  dandoDeBajaId = signal<number | null>(null);

  disponibles = computed(() => this.examenes().filter(e => !e.yaInscripto));
  inscriptas  = computed(() => this.examenes().filter(e => e.yaInscripto));
  private examenPendienteId: number | null = null;

  constructor(
    private service: MisExamenesService,
    private encuestasService: EncuestasService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.service.listarFinalesDisponibles().subscribe({
      next: data => { this.examenes.set(data); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar los exámenes disponibles.'); this.cargando.set(false); }
    });
  }

  inscribirse(examen: ExamenFinalDisponible): void {
    if (examen.yaInscripto || this.inscribiendo() !== null) return; // evita clicks repetidos en carrera
    this.error.set(null);
    this.inscribiendo.set(examen.id);

    this.encuestasService.obtenerPendiente().subscribe({
      next: (encuesta) => {
        if (encuesta) {
          this.inscribiendo.set(null);
          this.examenPendienteId = examen.id;
          this.encuestaPendiente.set(encuesta);
        } else {
          this.ejecutarInscripcion(examen.id);
        }
      },
      // No dejar pasar la inscripción si no se pudo verificar la encuesta obligatoria — mismo
      // fix que ffe69ff aplicó en mis-materias.component.ts, que este componente hermano nunca
      // recibió (ver CHECKLIST.md, Tier 7 #30). El backend igual bloquearía con 428, pero acá el
      // usuario vería un error genérico en vez de la encuesta que tenía que completar.
      error: () => {
        this.inscribiendo.set(null);
        this.error.set('No se pudo verificar si tenés una encuesta pendiente. Reintentá en unos segundos.');
      }
    });
  }

  onEncuestaCompletada(): void {
    const id = this.examenPendienteId;
    this.encuestaPendiente.set(null);
    this.examenPendienteId = null;
    if (id !== null) this.ejecutarInscripcion(id);
  }

  cerrarEncuestaPendiente(): void {
    this.encuestaPendiente.set(null);
    this.examenPendienteId = null;
    this.inscribiendo.set(null);
  }

  private ejecutarInscripcion(examenId: number): void {
    this.inscribiendo.set(examenId);
    this.service.inscribirse(examenId).subscribe({
      next: (resultado) => {
        this.examenes.update(list =>
          list.map(e => e.id === examenId ? { ...e, yaInscripto: true } : e)
        );
        this.inscribiendo.set(null);
        this.service.obtenerComprobante(resultado.id).subscribe({
          next: (c) => this.comprobante.set(c),
          error: () => {}
        });
      },
      error: (e) => {
        this.error.set(e.error?.detail ?? e.error?.mensaje ?? 'Error al inscribirse.');
        this.inscribiendo.set(null);
      }
    });
  }

  darDeBaja(examen: ExamenFinalDisponible): void {
    if (!examen.inscripcionId || this.dandoDeBajaId() !== null) return;

    const confirmado = confirm(
      `¿Dar de baja tu inscripción al examen final de "${examen.materiaNombre}"?\n\nEsta acción no se puede deshacer.`
    );
    if (!confirmado) return;

    const inscripcionId = examen.inscripcionId;
    this.dandoDeBajaId.set(inscripcionId);
    this.service.darDeBaja(inscripcionId).subscribe({
      next: () => {
        this.dandoDeBajaId.set(null);
        this.cargar();
      },
      error: (e) => {
        this.dandoDeBajaId.set(null);
        this.error.set(e.error?.detail ?? 'Error al dar de baja la inscripción.');
      }
    });
  }

  cerrarComprobante(): void { this.comprobante.set(null); }

  imprimirComprobante(): void { window.print(); }

  irAlDashboard(): void { this.router.navigate(['/dashboard']); }

  estadoLabel(estado: string): string {
    const map: Record<string, string> = {
      'activa':      '● Activa',
      'aprobada':    '✓ Aprobada',
      'desaprobada': '✗ Desaprobada',
      'baja':        '✗ Baja',
    };
    return map[estado.toLowerCase()] ?? estado;
  }
}
