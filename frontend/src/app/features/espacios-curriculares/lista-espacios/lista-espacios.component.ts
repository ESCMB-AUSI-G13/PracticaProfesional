import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { EspaciosCurricularesService, EspacioCurricular, CrearEspacioCurricularRequest } from '../espacios-curriculares.service';
import { MateriasService, Materia } from '../../materias/materias.service';
import { DocentesService, Docente } from '../../docentes/docentes.service';
import { CursosService, Curso } from '../../cursos/cursos.service';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-lista-espacios',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './lista-espacios.component.html',
  styleUrl: './lista-espacios.component.scss'
})
export class ListaEspaciosComponent implements OnInit {
  espacios  = signal<EspacioCurricular[]>([]);
  materias  = signal<Materia[]>([]);
  docentes  = signal<Docente[]>([]);
  cursos    = signal<Curso[]>([]);

  cargando  = signal(true);
  mostrarForm = signal(false);
  guardando = signal(false);
  error     = signal<string | null>(null);

  nuevoMateriaId  = signal<number | null>(null);
  nuevoDocenteId  = signal<number | null>(null);
  nuevoCursoId    = signal<number | null>(null);

  constructor(
    private service: EspaciosCurricularesService,
    private materiasService: MateriasService,
    private docentesService: DocentesService,
    private cursosService: CursosService,
    private router: Router
  ) {}

  ngOnInit(): void {
    forkJoin({
      espacios: this.service.listar(),
      materias: this.materiasService.listar(),
      docentes: this.docentesService.listar(),
      cursos:   this.cursosService.listar()
    }).subscribe({
      next: ({ espacios, materias, docentes, cursos }) => {
        this.espacios.set(espacios);
        this.materias.set(materias);
        this.docentes.set(docentes.filter(d => d.activo));
        this.cursos.set(cursos.filter(c => c.estado === 'Activo'));
        this.cargando.set(false);
      },
      error: () => { this.error.set('Error al cargar datos.'); this.cargando.set(false); }
    });
  }

  guardar(): void {
    if (!this.nuevoMateriaId() || !this.nuevoDocenteId() || !this.nuevoCursoId()) {
      this.error.set('Seleccioná materia, docente y curso.');
      return;
    }
    this.guardando.set(true);
    this.error.set(null);
    const dto: CrearEspacioCurricularRequest = {
      materiaId: this.nuevoMateriaId()!,
      docenteId: this.nuevoDocenteId()!,
      cursoId:   this.nuevoCursoId()!
    };
    this.service.crear(dto).subscribe({
      next: ec => {
        this.espacios.update(list => [...list, ec]);
        this.limpiarForm();
        this.guardando.set(false);
      },
      error: (e) => { this.error.set(e.error?.mensaje ?? 'Error al crear la cátedra.'); this.guardando.set(false); }
    });
  }

  eliminar(id: number): void {
    if (!confirm('¿Eliminar esta cátedra?')) return;
    this.service.eliminar(id).subscribe({
      next: () => this.espacios.update(list => list.filter(e => e.id !== id)),
      error: () => this.error.set('Error al eliminar.')
    });
  }

  limpiarForm(): void {
    this.nuevoMateriaId.set(null);
    this.nuevoDocenteId.set(null);
    this.nuevoCursoId.set(null);
    this.mostrarForm.set(false);
  }

  irAlDashboard(): void { this.router.navigate(['/dashboard']); }
}
