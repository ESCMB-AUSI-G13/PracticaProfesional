import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CalendarioService, TIPOS_EVENTO } from '../calendario.service';
import { MateriasService, Materia } from '../../materias/materias.service';
import { CursosService, Curso } from '../../cursos/cursos.service';
import { CarrerasService, Carrera } from '../../carreras/carreras.service';

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
  readonly TIPO_LIMITE_CARGA_NOTAS = 6;
  // Tipos donde Materia/Curso tienen sentido: en InscripcionMateria/InscripcionExamen acotan el
  // período a esa cátedra puntual (CalendarioAcademicoRepository.EstaEnPeriodoAsync ya lo valida
  // así); en FechaLimiteCargaNotas determinan a qué docente avisarle (CHECKLIST.md, Tier 6 #27).
  // Antes el formulario solo los mostraba para FechaLimiteCargaNotas, así que no había forma de
  // crear un período de inscripción acotado a una materia/curso aunque el backend lo soportaba.
  readonly TIPOS_CON_MATERIA_CURSO = [4, 5, 6];

  // Campos del formulario
  nombreEvento = signal('');
  comision     = signal('Todos');
  fechaInicio  = signal('');
  fechaFin     = signal('');
  tipoEvento   = signal<number>(8);
  materiaId    = signal<number | null>(null);
  cursoId      = signal<number | null>(null);

  carrerasOpciones = ['Todos', 'Trayecto', 'Profesorado'];
  carreras: Carrera[] = [];
  materias: Materia[] = [];
  cursos: Curso[] = [];

  constructor(
    private calendarioService: CalendarioService,
    private materiasService: MateriasService,
    private cursosService: CursosService,
    private carrerasService: CarrerasService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.materiasService.listar().subscribe(m => this.materias = m);
    this.cursosService.listar().subscribe(c => this.cursos = c);
    this.carrerasService.listar().subscribe(c => this.carreras = c);
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
          this.materiaId.set(e.materiaId ?? null);
          this.cursoId.set(e.cursoId ?? null);
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
      tipoEvento:   this.tipoEvento(),
      materiaId:    this.mostrarMateriaCurso() ? this.materiaId() : null,
      cursoId:      this.mostrarMateriaCurso() ? this.cursoId() : null
    };

    const op = this.esEdicion
      ? this.calendarioService.modificar(this.eventoId, dto)
      : this.calendarioService.crear(dto);

    op.subscribe({
      next: () => this.router.navigate(['/calendario']),
      error: (e) => { this.error.set(e.error?.mensaje ?? 'Error al guardar.'); this.guardando.set(false); }
    });
  }

  mostrarMateriaCurso(): boolean {
    return this.TIPOS_CON_MATERIA_CURSO.includes(this.tipoEvento());
  }

  // "Comision" es un texto libre (no una FK real a Carrera — se guarda tal cual en el evento),
  // así que para filtrar Materia/Curso por carrera hace falta emparejarlo contra el nombre real
  // de la carrera. Antes esta pantalla ni siquiera intentaba ese cruce: el combo de Materia
  // mostraba las de todas las carreras sin importar qué "Carrera" estuviera elegida.
  private carreraSeleccionadaId(): number | null {
    const etiqueta = this.comision();
    if (etiqueta === 'Todos') return null;
    return this.carreras.find(c => c.nombre.toLowerCase().includes(etiqueta.toLowerCase()))?.id ?? null;
  }

  materiasFiltradas(): Materia[] {
    const carreraId = this.carreraSeleccionadaId();
    return carreraId === null ? this.materias : this.materias.filter(m => m.carreraId === carreraId);
  }

  cursosFiltrados(): Curso[] {
    const carreraId = this.carreraSeleccionadaId();
    return carreraId === null ? this.cursos : this.cursos.filter(c => c.carreraId === carreraId);
  }

  onCarreraChange(valor: string): void {
    this.comision.set(valor);
    // La materia/curso ya elegidas pueden no pertenecer a la nueva carrera — se resetean para
    // no terminar guardando una combinación inconsistente (ej. Carrera=Profesorado con una
    // Materia que en realidad es de Trayecto).
    this.materiaId.set(null);
    this.cursoId.set(null);
  }

  cancelar(): void { this.router.navigate(['/calendario']); }
}
