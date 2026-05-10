import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MateriasService, Materia } from '../materias.service';
import { CorrelativiadadesService, Correlatividad } from '../correlatividades.service';
import { CarrerasService, Carrera } from '../../carreras/carreras.service';

@Component({
  selector: 'app-editar-materia',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './editar-materia.component.html',
  styleUrl: './editar-materia.component.scss'
})
export class EditarMateriaComponent implements OnInit {
  id        = 0;
  codigo    = signal('');
  nombre    = signal('');
  carreraId = signal<number | null>(null);
  anio      = signal<number>(1);
  cargando  = signal(true);
  guardando = signal(false);
  error     = signal<string | null>(null);

  carreras = signal<Carrera[]>([]);

  // Correlatividades
  todasLasMaterias     = signal<Materia[]>([]);
  correlatividades     = signal<Correlatividad[]>([]);
  cargandoCorr         = signal(false);
  errorCorr            = signal<string | null>(null);
  agregandoCorr        = signal(false);

  // Formulario nueva correlatividad
  nuevaRequisitoId    = signal<number | null>(null);
  nuevaTipo           = signal<'Cursar' | 'Rendir'>('Cursar');
  nuevaCondicion      = signal<number>(1);

  constructor(
    private materiasService: MateriasService,
    private correlativiadadesService: CorrelativiadadesService,
    private carrerasService: CarrerasService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));

    this.carrerasService.listar().subscribe({
      next: data => this.carreras.set(data),
      error: () => this.error.set('Error al cargar las carreras.')
    });

    this.materiasService.listar().subscribe({
      next: data => {
        const m = data.find(x => x.id === this.id);
        if (!m) { this.router.navigate(['/materias']); return; }
        this.codigo.set(m.codigo);
        this.nombre.set(m.nombre);
        this.carreraId.set(m.carreraId);
        this.anio.set(m.anio);
        this.todasLasMaterias.set(data.filter(x => x.id !== this.id));
        this.cargando.set(false);
        this.cargarCorrelatividades();
      },
      error: () => { this.error.set('Error al cargar la materia.'); this.cargando.set(false); }
    });
  }

  guardar(): void {
    if (!this.nombre() || !this.carreraId()) { this.error.set('Todos los campos son obligatorios.'); return; }
    this.guardando.set(true);
    this.error.set(null);
    this.materiasService.modificar(this.id, { nombre: this.nombre(), carreraId: this.carreraId()!, anio: this.anio() }).subscribe({
      next: () => this.router.navigate(['/materias']),
      error: (e) => { this.error.set(e.error?.detail ?? 'Error al guardar.'); this.guardando.set(false); }
    });
  }

  cancelar(): void { this.router.navigate(['/materias']); }

  // ── Correlatividades ─────────────────────────────────────────────────────────

  private cargarCorrelatividades(): void {
    this.cargandoCorr.set(true);
    this.correlativiadadesService.listarPorMateria(this.id).subscribe({
      next: data => { this.correlatividades.set(data); this.cargandoCorr.set(false); },
      error: () => { this.errorCorr.set('Error al cargar correlatividades.'); this.cargandoCorr.set(false); }
    });
  }

  agregarCorrelatividad(): void {
    const requisitoId = this.nuevaRequisitoId();
    if (!requisitoId) { this.errorCorr.set('Seleccioná una materia requisito.'); return; }

    this.agregandoCorr.set(true);
    this.errorCorr.set(null);

    this.correlativiadadesService.crear({
      materiaDestinoId:   this.id,
      materiaRequisitoId: requisitoId,
      tipoRequerimiento:  this.nuevaTipo(),
      condicionAcademica: this.nuevaCondicion() as 1 | 2
    }).subscribe({
      next: nueva => {
        this.correlatividades.update(list => [...list, nueva]);
        this.nuevaRequisitoId.set(null);
        this.nuevaTipo.set('Cursar');
        this.nuevaCondicion.set(1 as number);
        this.agregandoCorr.set(false);
      },
      error: (e) => {
        this.errorCorr.set(e.error?.detail ?? 'Error al agregar correlatividad.');
        this.agregandoCorr.set(false);
      }
    });
  }

  eliminarCorrelatividad(id: number): void {
    this.correlativiadadesService.eliminar(id).subscribe({
      next: () => this.correlatividades.update(list => list.filter(c => c.id !== id)),
      error: () => this.errorCorr.set('Error al eliminar la correlatividad.')
    });
  }

  condicionLabel(c: Correlatividad): string {
    return c.condicionAcademica === 'Aprobado' ? 'Aprobada' : 'Regularizada';
  }
}
