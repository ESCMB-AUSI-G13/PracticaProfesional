import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../auth/services/auth.service';
import {
  AsistenciasService,
  EspacioAsistencia,
  RegistroDelDia,
  AsistenciaDetalle,
  MOTIVOS_INASISTENCIA,
} from '../asistencias.service';
import { EspaciosCurricularesService, EspacioCurricular } from '../../espacios-curriculares/espacios-curriculares.service';

interface FilaRectificacion extends AsistenciaDetalle {
  estadoEditado: string;
  motivoOpcion:  string;
  motivoTexto:   string;
  modificado:    boolean;
}

@Component({
  selector: 'app-rectificar-asistencias',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './rectificar-asistencias.component.html',
  styleUrl: './rectificar-asistencias.component.scss'
})
export class RectificarAsistenciasComponent implements OnInit {
  readonly rolReal = computed(() => this.authService.rol());
  readonly esDocente = computed(() => this.rolReal() === 'Docente');

  // ── Selección de espacio ─────────────────────────────────────────────────────
  espacios        = signal<EspacioAsistencia[] | EspacioCurricular[]>([]);
  cargandoEspacios = signal(false);
  espacioSeleccionado = signal<{ id: number; label: string } | null>(null);

  // ── Selección de fecha ───────────────────────────────────────────────────────
  fecha = signal(this.hoyISO());

  // ── Registro cargado ─────────────────────────────────────────────────────────
  filas         = signal<FilaRectificacion[]>([]);
  cargandoRegistro = signal(false);
  registroCargado  = signal(false);

  // ── Guardado ─────────────────────────────────────────────────────────────────
  guardando = signal(false);
  error     = signal<string | null>(null);
  exito     = signal(false);

  hayModificaciones = computed(() => this.filas().some(f => f.modificado));

  readonly motivos = MOTIVOS_INASISTENCIA;

  constructor(
    private authService: AuthService,
    private asistenciasService: AsistenciasService,
    private espaciosService: EspaciosCurricularesService
  ) {}

  ngOnInit(): void {
    this.cargarEspacios();
  }

  // ── Carga de espacios según rol ──────────────────────────────────────────────

  cargarEspacios(): void {
    this.cargandoEspacios.set(true);
    if (this.esDocente()) {
      this.asistenciasService.obtenerMisEspacios().subscribe({
        next: data => { this.espacios.set(data); this.cargandoEspacios.set(false); },
        error: (err) => {
          this.error.set(err?.error?.detail ?? 'No se pudieron cargar las cátedras.');
          this.cargandoEspacios.set(false);
        }
      });
    } else {
      this.espaciosService.listar().subscribe({
        next: data => { this.espacios.set(data); this.cargandoEspacios.set(false); },
        error: () => { this.error.set('No se pudieron cargar los espacios.'); this.cargandoEspacios.set(false); }
      });
    }
  }

  seleccionarEspacio(id: number, label: string): void {
    this.espacioSeleccionado.set({ id, label });
    this.limpiarRegistro();
  }

  // ── Helpers de tipado para la template ──────────────────────────────────────

  comoDocente(e: unknown): EspacioAsistencia { return e as EspacioAsistencia; }
  comoDireccion(e: unknown): EspacioCurricular { return e as EspacioCurricular; }

  // ── Carga del registro del día ───────────────────────────────────────────────

  cargarRegistro(): void {
    const espacio = this.espacioSeleccionado();
    if (!espacio || !this.fecha()) return;

    this.cargandoRegistro.set(true);
    this.error.set(null);
    this.exito.set(false);

    this.asistenciasService.obtenerRegistroDelDia(espacio.id, this.fecha()).subscribe({
      next: (registro: RegistroDelDia) => {
        this.filas.set(registro.alumnos.map(a => {
          const motivoGuardado = a.motivo ?? '';
          const motivoOpcion = !motivoGuardado ? ''
            : MOTIVOS_INASISTENCIA.slice(0, -1).includes(motivoGuardado) ? motivoGuardado
            : 'Otro';
          return {
            ...a,
            estadoEditado: a.estado,
            motivoOpcion,
            motivoTexto: motivoOpcion === 'Otro' ? motivoGuardado : '',
            modificado: false,
          };
        }));
        this.registroCargado.set(true);
        this.cargandoRegistro.set(false);
      },
      error: (err) => {
        this.error.set(err?.error?.detail ?? 'No hay registro de asistencia para esa fecha.');
        this.registroCargado.set(false);
        this.cargandoRegistro.set(false);
      }
    });
  }

  // ── Lógica de edición ────────────────────────────────────────────────────────

  private motivoFinal(fila: FilaRectificacion): string {
    if (fila.estadoEditado !== 'AusenteJustificado') return '';
    return fila.motivoOpcion === 'Otro'
      ? (fila.motivoTexto.trim() || 'Otro')
      : fila.motivoOpcion;
  }

  onEstadoChange(fila: FilaRectificacion): void {
    if (fila.estadoEditado !== 'AusenteJustificado') {
      fila.motivoOpcion = '';
      fila.motivoTexto = '';
    }
    fila.modificado = fila.estadoEditado !== fila.estado || this.motivoFinal(fila) !== (fila.motivo ?? '');
    this.filas.set([...this.filas()]);
  }

  onMotivoChange(fila: FilaRectificacion): void {
    fila.modificado = fila.estadoEditado !== fila.estado || this.motivoFinal(fila) !== (fila.motivo ?? '');
    this.filas.set([...this.filas()]);
  }

  // ── Guardar rectificación ────────────────────────────────────────────────────

  guardar(): void {
    const espacio = this.espacioSeleccionado();
    if (!espacio) return;

    for (const fila of this.filas()) {
      if (fila.estadoEditado === 'AusenteJustificado' && !fila.motivoOpcion) {
        this.error.set('Seleccioná el motivo para todas las ausencias justificadas.');
        return;
      }
    }

    this.error.set(null);
    this.guardando.set(true);

    const cambios = this.filas()
      .filter(f => f.modificado)
      .map(f => ({
        asistenciaId: f.asistenciaId,
        nuevoEstado: f.estadoEditado,
        motivo: this.motivoFinal(f) || undefined,
      }));

    this.asistenciasService.rectificarAsistencias({
      espacioCurricularId: espacio.id,
      fecha: this.fecha(),
      cambios
    }).subscribe({
      next: () => {
        this.filas.update(filas => filas.map(f => ({
          ...f,
          estado: f.estadoEditado,
          motivo: this.motivoFinal(f) || null,
          modificado: false,
        })));
        this.exito.set(true);
        this.guardando.set(false);
      },
      error: (err) => {
        this.error.set(err?.error?.detail ?? 'Error al guardar la rectificación.');
        this.guardando.set(false);
      }
    });
  }

  // ── Reset ────────────────────────────────────────────────────────────────────

  limpiarRegistro(): void {
    this.filas.set([]);
    this.registroCargado.set(false);
    this.error.set(null);
    this.exito.set(false);
  }

  nuevaBusqueda(): void {
    this.espacioSeleccionado.set(null);
    this.limpiarRegistro();
  }

  etiquetaEstado(estado: string): string {
    const map: Record<string, string> = {
      Presente: 'Presente',
      Ausente: 'Ausente injustificado',
      AusenteJustificado: 'Ausente justificado'
    };
    return map[estado] ?? estado;
  }

  hoyISO(): string {
    return new Date().toISOString().substring(0, 10);
  }
}
