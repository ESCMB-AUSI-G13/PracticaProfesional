import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { CursosService } from '../cursos.service';
import { PreceptoresService, Preceptor } from '../../preceptores/preceptores.service';

@Component({
  selector: 'app-editar-curso',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './editar-curso.component.html',
  styleUrl: './editar-curso.component.scss'
})
export class EditarCursoComponent implements OnInit {
  id          = 0;
  anio        = signal(0);
  comision    = signal('');
  cupo        = signal(0);
  // Guarda el usuarioId del preceptor seleccionado (así lo espera la API, igual que en creación).
  preceptorUsuarioId = signal<number | null>(null);

  preceptores = signal<Preceptor[]>([]);

  cargando  = signal(true);
  guardando = signal(false);
  error     = signal<string | null>(null);

  constructor(
    private cursosService: CursosService,
    private preceptoresService: PreceptoresService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    forkJoin({
      preceptores: this.preceptoresService.listar(),
      cursos: this.cursosService.listar()
    }).subscribe({
      next: ({ preceptores, cursos }) => {
        const c = cursos.find(x => x.id === this.id);
        if (!c) { this.router.navigate(['/cursos']); return; }

        // El preceptor actual del curso puede estar inactivo; si es así, lo incluimos
        // igual en las opciones para no perder la selección vigente.
        const actual = preceptores.find(p => p.id === c.preceptorId);
        const opciones = preceptores.filter(p => p.activo);
        if (actual && !actual.activo) opciones.push(actual);
        this.preceptores.set(opciones);

        this.anio.set(c.anio);
        this.comision.set(c.comision);
        this.cupo.set(c.cupo);
        this.preceptorUsuarioId.set(actual?.usuarioId ?? null);
        this.cargando.set(false);
      },
      error: () => { this.error.set('Error al cargar el curso.'); this.cargando.set(false); }
    });
  }

  guardar(): void {
    if (!this.comision() || this.cupo() <= 0 || !this.preceptorUsuarioId()) {
      this.error.set('Comisión, cupo y preceptor son obligatorios.');
      return;
    }
    this.guardando.set(true);
    this.error.set(null);
    this.cursosService.modificar(this.id, {
      comision: this.comision(),
      cupo: this.cupo(),
      preceptorUsuarioId: this.preceptorUsuarioId()!
    }).subscribe({
      next: () => this.router.navigate(['/cursos']),
      error: (e) => { this.error.set(e.error?.mensaje ?? 'Error al guardar.'); this.guardando.set(false); }
    });
  }

  cancelar(): void { this.router.navigate(['/cursos']); }
}
