import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MisExamenesService, ExamenFinalDisponible, ComprobanteInscripcionExamen } from './mis-examenes.service';
import { EncuestasService, EncuestaDto } from '../encuestas/encuestas.service';
import { ModalEncuestaComponent } from '../encuestas/modal-encuesta/modal-encuesta.component';

@Component({
  selector: 'app-mis-examenes',
  standalone: true,
  imports: [CommonModule, ModalEncuestaComponent],
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
    if (examen.yaInscripto || this.inscribiendo() !== null) return;
    this.error.set(null);

    this.encuestasService.obtenerPendiente().subscribe({
      next: (encuesta) => {
        if (encuesta) {
          this.examenPendienteId = examen.id;
          this.encuestaPendiente.set(encuesta);
        } else {
          this.ejecutarInscripcion(examen.id);
        }
      },
      error: () => this.ejecutarInscripcion(examen.id)
    });
  }

  onEncuestaCompletada(): void {
    const id = this.examenPendienteId;
    this.encuestaPendiente.set(null);
    this.examenPendienteId = null;
    if (id !== null) this.ejecutarInscripcion(id);
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
        this.error.set(e.error?.mensaje ?? 'Error al inscribirse.');
        this.inscribiendo.set(null);
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
