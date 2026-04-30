import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  CalificacionesService,
  InscripcionExamenDto,
  CambioNotaDto
} from '../calificaciones.service';

interface FilaActa extends InscripcionExamenDto {
  notaInput:       string;
  guardando:       boolean;
  errorFila:       string | null;
  guardado:        boolean;
  // Rectificación inline
  rectificando:    boolean;
  rectificaInput:  string;
  motivoInput:     string;
  errorRectifica:  string | null;
}

@Component({
  selector: 'app-carga-notas',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './carga-notas.component.html',
  styleUrl: './carga-notas.component.scss'
})
export class CargaNotasComponent {
  examenId  = signal<number | null>(null);
  filas     = signal<FilaActa[]>([]);
  cargando  = signal(false);
  error     = signal<string | null>(null);
  buscado   = signal(false);

  // Panel historial
  historialFila     = signal<FilaActa | null>(null);
  historial         = signal<CambioNotaDto[]>([]);
  cargandoHistorial = signal(false);

  constructor(
    private calificacionesService: CalificacionesService,
    private router: Router
  ) {}

  // ── Buscar acta ─────────────────────────────────────────────────────────────

  buscarActa(): void {
    const id = this.examenId();
    if (!id || id <= 0) { this.error.set('Ingresá un ID de examen válido.'); return; }

    this.cargando.set(true);
    this.error.set(null);
    this.buscado.set(false);
    this.filas.set([]);
    this.cerrarHistorial();

    this.calificacionesService.listarInscripciones(id).subscribe({
      next: data => {
        this.filas.set(data.map(i => ({
          ...i,
          notaInput: '', guardando: false, errorFila: null, guardado: false,
          rectificando: false, rectificaInput: '', motivoInput: '', errorRectifica: null
        })));
        this.buscado.set(true);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('No se pudo obtener el acta. Verificá que el ID de examen sea correcto.');
        this.cargando.set(false);
      }
    });
  }

  // ── Carga inicial de nota ────────────────────────────────────────────────────

  guardarNota(fila: FilaActa): void {
    const valor = parseFloat(fila.notaInput);
    if (isNaN(valor) || valor < 1 || valor > 10) {
      fila.errorFila = 'La nota debe estar entre 1 y 10.';
      return;
    }
    fila.errorFila = null;
    fila.guardando = true;

    this.calificacionesService.cargarNota(fila.id, valor).subscribe({
      next: res => {
        fila.notaValor  = res.notaValor;
        fila.esAprobado = res.esAprobado;
        fila.estado     = res.estado;
        fila.guardando  = false;
        fila.guardado   = true;
        fila.notaInput  = '';
      },
      error: err => {
        fila.errorFila = err?.error?.detail ?? err?.error?.title ?? 'Error al guardar la nota.';
        fila.guardando = false;
      }
    });
  }

  // ── Rectificación ────────────────────────────────────────────────────────────

  abrirRectificacion(fila: FilaActa): void {
    // Cerrar rectificación abierta en otra fila
    this.filas().forEach(f => {
      if (f !== fila) { f.rectificando = false; f.rectificaInput = ''; f.motivoInput = ''; f.errorRectifica = null; }
    });
    fila.rectificando   = !fila.rectificando;
    fila.rectificaInput = '';
    fila.motivoInput    = '';
    fila.errorRectifica = null;
  }

  confirmarRectificacion(fila: FilaActa): void {
    const valor = parseFloat(fila.rectificaInput);
    if (isNaN(valor) || valor < 1 || valor > 10) {
      fila.errorRectifica = 'La nota debe estar entre 1 y 10.';
      return;
    }
    if (!fila.motivoInput.trim()) {
      fila.errorRectifica = 'El motivo de rectificación es obligatorio.';
      return;
    }

    fila.errorRectifica = null;
    fila.guardando = true;

    this.calificacionesService.rectificarNota(fila.id, valor, fila.motivoInput.trim()).subscribe({
      next: res => {
        fila.notaValor      = res.notaValor;
        fila.esAprobado     = res.esAprobado;
        fila.estado         = res.estado;
        fila.rectificando   = false;
        fila.rectificaInput = '';
        fila.motivoInput    = '';
        fila.guardando      = false;
        // Si el panel de historial estaba abierto para esta fila, recargar
        if (this.historialFila()?.id === fila.id) this.cargarHistorial(fila);
      },
      error: err => {
        fila.errorRectifica = err?.error?.detail ?? err?.error?.title ?? 'Error al rectificar la nota.';
        fila.guardando = false;
      }
    });
  }

  cancelarRectificacion(fila: FilaActa): void {
    fila.rectificando   = false;
    fila.rectificaInput = '';
    fila.motivoInput    = '';
    fila.errorRectifica = null;
  }

  // ── Historial ────────────────────────────────────────────────────────────────

  verHistorial(fila: FilaActa): void {
    if (this.historialFila()?.id === fila.id) { this.cerrarHistorial(); return; }
    this.cargarHistorial(fila);
  }

  private cargarHistorial(fila: FilaActa): void {
    this.historialFila.set(fila);
    this.historial.set([]);
    this.cargandoHistorial.set(true);

    this.calificacionesService.obtenerHistorial(fila.id).subscribe({
      next: data => { this.historial.set(data); this.cargandoHistorial.set(false); },
      error: ()  => { this.historial.set([]); this.cargandoHistorial.set(false); }
    });
  }

  cerrarHistorial(): void {
    this.historialFila.set(null);
    this.historial.set([]);
  }

  // ── Helpers ──────────────────────────────────────────────────────────────────

  parsearJson(json: string | null): string {
    if (!json) return '—';
    try {
      const obj = JSON.parse(json);
      return Object.entries(obj)
        .map(([k, v]) => `${k}: ${v}`)
        .join(' | ');
    } catch { return json; }
  }

  limpiar(): void {
    this.examenId.set(null);
    this.filas.set([]);
    this.buscado.set(false);
    this.error.set(null);
    this.cerrarHistorial();
  }

  irAlDashboard(): void { this.router.navigate(['/dashboard']); }

  get pendientes(): number  { return this.filas().filter(f => f.estado === 'Activa').length; }
  get calificados(): number { return this.filas().filter(f => f.estado !== 'Activa' && f.estado !== 'Baja').length; }

  estadoLabel(estado: string): string {
    const map: Record<string, string> = {
      'activa':      '— Pendiente',
      'aprobada':    '✓ Aprobada',
      'desaprobada': '✗ Desaprobada',
      'baja':        '✗ Baja',
    };
    return map[estado.toLowerCase()] ?? estado;
  }
}
