import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  CalificacionesService,
  ExamenResumen,
  InscripcionExamenDto,
  CambioNotaDto
} from '../calificaciones.service';

interface FilaActa extends InscripcionExamenDto {
  notaInput:       string;
  guardando:       boolean;
  errorFila:       string | null;
  guardado:        boolean;
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
export class CargaNotasComponent implements OnInit {
  // ── Lista de exámenes ────────────────────────────────────────────────────────
  examenes          = signal<ExamenResumen[]>([]);
  cargandoLista     = signal(false);
  filtroBusqueda    = signal('');
  filtroFecha       = signal('');
  examenSeleccionado = signal<ExamenResumen | null>(null);

  examenesVisibles = computed(() => {
    const texto = this.filtroBusqueda().toLowerCase().trim();
    const fecha = this.filtroFecha();
    return this.examenes().filter(e => {
      const matchNombre = !texto || e.materiaNombre.toLowerCase().includes(texto);
      const matchFecha  = !fecha  || e.fechaExamen.substring(0, 10) === fecha;
      return matchNombre && matchFecha;
    });
  });

  // ── Acta del examen ──────────────────────────────────────────────────────────
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

  ngOnInit(): void {
    this.cargarExamenes();
  }

  // ── Listado de exámenes ──────────────────────────────────────────────────────

  cargarExamenes(): void {
    this.cargandoLista.set(true);
    this.calificacionesService.listarExamenes().subscribe({
      next: data => {
        // Ordenar por fecha descendente
        this.examenes.set(data.sort((a, b) =>
          new Date(b.fechaExamen).getTime() - new Date(a.fechaExamen).getTime()
        ));
        this.cargandoLista.set(false);
      },
      error: () => this.cargandoLista.set(false)
    });
  }

  seleccionarExamen(examen: ExamenResumen): void {
    if (this.examenSeleccionado()?.id === examen.id) return;
    this.examenSeleccionado.set(examen);
    this.cargarActa(examen.id);
  }

  // ── Cargar acta ──────────────────────────────────────────────────────────────

  private cargarActa(examenId: number): void {
    this.cargando.set(true);
    this.error.set(null);
    this.buscado.set(false);
    this.filas.set([]);
    this.cerrarHistorial();

    this.calificacionesService.listarInscripciones(examenId).subscribe({
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
        this.error.set('No se pudo cargar el acta del examen seleccionado.');
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
      return Object.entries(obj).map(([k, v]) => `${k}: ${v}`).join(' | ');
    } catch { return json; }
  }

  limpiar(): void {
    this.examenSeleccionado.set(null);
    this.filtroBusqueda.set('');
    this.filtroFecha.set('');
    this.filas.set([]);
    this.buscado.set(false);
    this.error.set(null);
    this.cerrarHistorial();
  }

  tipoBadgeClass(tipo: string): string {
    return { Parcial: 'tipo-parcial', Final: 'tipo-final', Recuperatorio: 'tipo-recuperatorio' }[tipo] ?? '';
  }

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
