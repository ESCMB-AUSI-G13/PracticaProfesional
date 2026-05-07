import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CalendarioService, TIPOS_EVENTO } from '../calendario.service';

@Component({
  selector: 'app-crear-evento',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './crear-evento.component.html',
  styleUrl: './crear-evento.component.scss'
})
export class CrearEventoComponent implements OnInit {
  esEdicion  = false;
  eventoId   = 0;
  cargando   = signal(false);
  guardando  = signal(false);
  error      = signal<string | null>(null);

  tiposEvento = TIPOS_EVENTO;

  // Campos del formulario
  nombreEvento = signal('');
  comision     = signal('Todos');
  fechaInicio  = signal('');
  fechaFin     = signal('');
  tipoEvento   = signal<number>(8);

  carrerasOpciones = ['Todos', 'Trayecto', 'Profesorado'];

  constructor(
    private calendarioService: CalendarioService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.esEdicion = true;
      this.eventoId  = Number(id);
      this.cargando.set(true);
      this.calendarioService.listar().subscribe({
        next: lista => {
          const e = lista.find(x => x.id === this.eventoId);
          if (!e) { this.router.navigate(['/calendario']); return; }
          this.nombreEvento.set(e.nombreEvento);
          this.comision.set(e.comision);
          this.fechaInicio.set(e.fechaInicio.split('T')[0]);
          this.fechaFin.set(e.fechaFin.split('T')[0]);
          const tipoMap: Record<string, number> = {
            InicioClases: 1, FinClases: 2, PeriodoExamen: 3,
            InscripcionMateria: 4, InscripcionExamen: 5,
            FechaLimiteCargaNotas: 6, Feriado: 7, Otro: 8
          };
          this.tipoEvento.set(tipoMap[e.tipoEvento] ?? 8);
          this.cargando.set(false);
        },
        error: () => { this.error.set('Error al cargar el evento.'); this.cargando.set(false); }
      });
    }
  }

  guardar(): void {
    if (!this.nombreEvento() || !this.fechaInicio() || !this.fechaFin()) {
      this.error.set('Completá todos los campos obligatorios.');
      return;
    }
    this.guardando.set(true);
    this.error.set(null);

    const dto = {
      nombreEvento: this.nombreEvento(),
      comision:     this.comision(),
      fechaInicio:  this.fechaInicio(),
      fechaFin:     this.fechaFin(),
      tipoEvento:   this.tipoEvento()
    };

    const op = this.esEdicion
      ? this.calendarioService.modificar(this.eventoId, dto)
      : this.calendarioService.crear(dto);

    op.subscribe({
      next: () => this.router.navigate(['/calendario']),
      error: (e) => { this.error.set(e.error?.mensaje ?? 'Error al guardar.'); this.guardando.set(false); }
    });
  }

  cancelar(): void { this.router.navigate(['/calendario']); }
}
