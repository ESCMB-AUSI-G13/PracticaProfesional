import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  AsistenciasService,
  EspacioAsistencia,
  AlumnoParaAsistencia,
  ResumenAsistencias,
} from '../asistencias.service';

interface FilaAlumno extends AlumnoParaAsistencia {
  ausente:      boolean;
  tipoAusencia: 'Injustificada' | 'Justificada';
  motivo:       string;
}

@Component({
  selector: 'app-registrar-asistencias',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './registrar-asistencias.component.html',
  styleUrl: './registrar-asistencias.component.scss'
})
export class RegistrarAsistenciasComponent implements OnInit {
  // ── Espacios curriculares ────────────────────────────────────────────────────
  espacios          = signal<EspacioAsistencia[]>([]);
  cargandoEspacios  = signal(false);
  espacioSeleccionado = signal<EspacioAsistencia | null>(null);

  // ── Lista de alumnos ─────────────────────────────────────────────────────────
  filas         = signal<FilaAlumno[]>([]);
  cargandoLista = signal(false);
  fecha         = signal(this.hoyISO());

  ausentesCount = computed(() => this.filas().filter(f => f.ausente).length);
  presentesCount = computed(() => this.filas().filter(f => !f.ausente).length);

  // ── Envío ────────────────────────────────────────────────────────────────────
  guardando  = signal(false);
  error      = signal<string | null>(null);
  resumen    = signal<ResumenAsistencias | null>(null);

  constructor(private service: AsistenciasService) {}

  ngOnInit(): void {
    this.cargarEspacios();
  }

  // ── Espacios ─────────────────────────────────────────────────────────────────

  cargarEspacios(): void {
    this.cargandoEspacios.set(true);
    this.service.obtenerMisEspacios().subscribe({
      next: data => { this.espacios.set(data); this.cargandoEspacios.set(false); },
      error: ()  => this.cargandoEspacios.set(false)
    });
  }

  seleccionarEspacio(espacio: EspacioAsistencia): void {
    if (this.espacioSeleccionado()?.espacioCurricularId === espacio.espacioCurricularId) return;
    this.espacioSeleccionado.set(espacio);
    this.resumen.set(null);
    this.error.set(null);
    this.cargarAlumnos(espacio.espacioCurricularId);
  }

  // ── Alumnos ──────────────────────────────────────────────────────────────────

  private cargarAlumnos(espacioCurricularId: number): void {
    this.cargandoLista.set(true);
    this.filas.set([]);
    this.service.obtenerAlumnosPorEspacio(espacioCurricularId).subscribe({
      next: data => {
        this.filas.set(data.map(a => ({
          ...a,
          ausente: false,
          tipoAusencia: 'Injustificada',
          motivo: ''
        })));
        this.cargandoLista.set(false);
      },
      error: () => this.cargandoLista.set(false)
    });
  }

  toggleAusente(fila: FilaAlumno): void {
    fila.ausente = !fila.ausente;
    if (!fila.ausente) {
      fila.tipoAusencia = 'Injustificada';
      fila.motivo = '';
    }
    this.filas.set([...this.filas()]);
  }

  // ── Registrar ────────────────────────────────────────────────────────────────

  registrar(): void {
    const espacio = this.espacioSeleccionado();
    if (!espacio || this.filas().length === 0) return;

    for (const fila of this.filas()) {
      if (fila.ausente && fila.tipoAusencia === 'Justificada' && !fila.motivo.trim()) {
        this.error.set('Completá el motivo para todas las ausencias justificadas.');
        return;
      }
    }

    this.error.set(null);
    this.guardando.set(true);

    const ausentes = this.filas()
      .filter(f => f.ausente)
      .map(f => ({
        estudianteId: f.estudianteId,
        tipoAusencia: f.tipoAusencia,
        motivo: f.motivo.trim() || undefined
      }));

    this.service.registrarAsistencias({
      espacioCurricularId: espacio.espacioCurricularId,
      fecha: this.fecha(),
      ausentes
    }).subscribe({
      next: res => {
        this.resumen.set(res);
        this.guardando.set(false);
      },
      error: err => {
        this.error.set(err?.error?.detail ?? err?.error?.title ?? 'Error al registrar la asistencia.');
        this.guardando.set(false);
      }
    });
  }

  // ── Nuevo registro ───────────────────────────────────────────────────────────

  nuevo(): void {
    this.espacioSeleccionado.set(null);
    this.filas.set([]);
    this.resumen.set(null);
    this.error.set(null);
    this.fecha.set(this.hoyISO());
  }

  // ── Helpers ──────────────────────────────────────────────────────────────────

  private hoyISO(): string {
    return new Date().toISOString().substring(0, 10);
  }

  etiquetaEspacio(e: EspacioAsistencia): string {
    return `${e.materiaNombre} — Año ${e.anioLectivo} Comisión ${e.comision}`;
  }
}
