import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CalendarioService, EventoCalendario, etiquetaTipo } from '../calendario.service';
import { AuthService } from '../../auth/services/auth.service';

interface DiaCalendario {
  dia: number | null;
  clase: string;
  tooltip: string;
  esHoy: boolean;
}

interface MesCalendario {
  nombre: string;
  dias: DiaCalendario[];
}

@Component({
  selector: 'app-lista-calendario',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './lista-calendario.component.html',
  styleUrl: './lista-calendario.component.scss'
})
export class ListaCalendarioComponent implements OnInit {
  private authService = inject(AuthService);
  esDireccion = computed(() => this.authService.rol() === 'Direccion');

  anioFiltro   = signal(new Date().getFullYear());
  eventos      = signal<EventoCalendario[]>([]);
  cargando     = signal(true);
  error        = signal<string | null>(null);
  eliminandoId = signal<number | null>(null);
  vistaActual  = signal<'lista' | 'calendario'>('lista');

  aniosDisponibles = [2025, 2026, 2027];
  diasSemana = ['L', 'M', 'M', 'J', 'V', 'S', 'D'];

  private readonly NOMBRES_MESES = [
    'Enero','Febrero','Marzo','Abril','Mayo','Junio',
    'Julio','Agosto','Septiembre','Octubre','Noviembre','Diciembre'
  ];

  leyenda = [
    { clase: 'badge-inicio',        label: 'Inicio de clases' },
    { clase: 'badge-fin',           label: 'Fin de clases' },
    { clase: 'badge-inscr-materia', label: 'Inscripción a materias' },
    { clase: 'badge-inscr-examen',  label: 'Inscripción a exámenes' },
    { clase: 'badge-examen',        label: 'Período de exámenes' },
    { clase: 'badge-limite',        label: 'Límite carga de notas' },
    { clase: 'badge-feriado',       label: 'Feriado' },
    { clase: 'badge-otro',          label: 'Otro' },
  ];

  meses = computed<MesCalendario[]>(() => {
    const anio = this.anioFiltro();
    const evs  = this.eventos();
    const hoy  = new Date();

    return this.NOMBRES_MESES.map((nombre, mesIdx) => {
      const diasEnMes = new Date(anio, mesIdx + 1, 0).getDate();
      let inicioSemana = new Date(anio, mesIdx, 1).getDay();
      inicioSemana = inicioSemana === 0 ? 6 : inicioSemana - 1;

      const dias: DiaCalendario[] = [];

      for (let i = 0; i < inicioSemana; i++) {
        dias.push({ dia: null, clase: '', tooltip: '', esHoy: false });
      }

      for (let d = 1; d <= diasEnMes; d++) {
        const ts = new Date(anio, mesIdx, d).getTime();
        let clase = '';
        const nombres: string[] = [];

        for (const e of evs) {
          const [sy, sm, sd] = e.fechaInicio.split('T')[0].split('-').map(Number);
          const [ey, em, ed] = e.fechaFin.split('T')[0].split('-').map(Number);
          const inicio = new Date(sy, sm - 1, sd).getTime();
          const fin    = new Date(ey, em - 1, ed).getTime();
          if (ts >= inicio && ts <= fin) {
            if (!clase) clase = this.claseTipo(e.tipoEvento);
            nombres.push(e.nombreEvento);
          }
        }

        const esHoy = anio === hoy.getFullYear() && mesIdx === hoy.getMonth() && d === hoy.getDate();
        dias.push({ dia: d, clase, tooltip: nombres.join('\n'), esHoy });
      }

      while (dias.length % 7 !== 0) {
        dias.push({ dia: null, clase: '', tooltip: '', esHoy: false });
      }

      return { nombre, dias };
    });
  });

  constructor(
    private calendarioService: CalendarioService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (!this.esDireccion()) this.vistaActual.set('calendario');
    this.cargar();
  }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.calendarioService.listar(this.anioFiltro()).subscribe({
      next: data => { this.eventos.set(data); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar el calendario.'); this.cargando.set(false); }
    });
  }

  irACrear(): void { this.router.navigate(['/calendario/nuevo']); }
  irAEditar(id: number): void { this.router.navigate(['/calendario', id, 'editar']); }

  eliminar(id: number): void {
    if (!confirm('¿Eliminar este evento del calendario?')) return;
    this.eliminandoId.set(id);
    this.calendarioService.eliminar(id).subscribe({
      next: () => { this.eventos.update(list => list.filter(e => e.id !== id)); this.eliminandoId.set(null); },
      error: () => { this.error.set('Error al eliminar el evento.'); this.eliminandoId.set(null); }
    });
  }

  etiqueta(tipo: string): string { return etiquetaTipo(tipo); }

  formatFecha(f: string): string {
    return new Date(f).toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  claseTipo(tipo: string): string {
    const mapa: Record<string, string> = {
      PeriodoExamen:         'badge-examen',
      InscripcionExamen:     'badge-inscr-examen',
      InscripcionMateria:    'badge-inscr-materia',
      InicioClases:          'badge-inicio',
      FinClases:             'badge-fin',
      FechaLimiteCargaNotas: 'badge-limite',
      Feriado:               'badge-feriado',
      Otro:                  'badge-otro',
    };
    return mapa[tipo] ?? 'badge-otro';
  }
}
