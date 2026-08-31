import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MiHistorialService, HistorialMateria } from './mi-historial.service';
import { CargandoComponent } from '../../shared/cargando/cargando.component';

type FiltroEstado = 'todos' | 'aprobado' | 'desaprobado';

const NUMEROS_EN_PALABRAS: Record<number, string> = {
  1: 'Uno', 2: 'Dos', 3: 'Tres', 4: 'Cuatro', 5: 'Cinco',
  6: 'Seis', 7: 'Siete', 8: 'Ocho', 9: 'Nueve', 10: 'Diez',
};

@Component({
  selector: 'app-mi-historial',
  standalone: true,
  imports: [CommonModule, FormsModule, CargandoComponent],
  templateUrl: './mi-historial.component.html',
  styleUrl: './mi-historial.component.scss'
})
export class MiHistorialComponent implements OnInit {
  materias        = signal<HistorialMateria[]>([]);
  promedioGeneral = signal<number | null>(null);
  cargando        = signal(true);
  error           = signal<string | null>(null);

  filtroTexto  = signal('');
  filtroAnio   = signal<number | null>(null);
  filtroEstado = signal<FiltroEstado>('todos');

  aniosDisponibles = computed(() =>
    [...new Set(this.materias().map(m => m.anio).filter((a): a is number => a !== null))]
      .sort((a, b) => b - a)
  );

  materiasFiltradas = computed(() => {
    const texto = this.filtroTexto().trim().toLowerCase();
    const anio = this.filtroAnio();
    const estado = this.filtroEstado();

    return this.materias().filter(m => {
      if (texto && !m.materiaNombre.toLowerCase().includes(texto) && !m.materiaCodigo.toLowerCase().includes(texto))
        return false;
      if (anio !== null && m.anio !== anio)
        return false;
      if (estado === 'aprobado' && m.estadoFinal !== 'Aprobada')
        return false;
      if (estado === 'desaprobado' && m.estadoFinal !== 'Desaprobada')
        return false;
      return true;
    });
  });

  constructor(private service: MiHistorialService) {}

  ngOnInit(): void {
    this.service.obtenerMiHistorial().subscribe({
      next: (data) => {
        this.materias.set(data.materias);
        this.promedioGeneral.set(data.promedioGeneral);
        this.cargando.set(false);
      },
      error: (e) => {
        this.error.set(e.error?.detail ?? 'Error al cargar tu historial académico.');
        this.cargando.set(false);
      }
    });
  }

  notaConPalabra(nota: number | null): string {
    if (nota === null) return '—';
    const entero = Number.isInteger(nota) ? nota : null;
    const palabra = entero !== null ? NUMEROS_EN_PALABRAS[entero] : null;
    return palabra ? `${nota} (${palabra})` : `${nota}`;
  }
}
