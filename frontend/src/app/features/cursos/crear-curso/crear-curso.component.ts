import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CursosService } from '../cursos.service';
import { PreceptoresService, Preceptor } from '../../preceptores/preceptores.service';

@Component({
  selector: 'app-crear-curso',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './crear-curso.component.html',
  styleUrl: './crear-curso.component.scss'
})
export class CrearCursoComponent implements OnInit {
  anio         = signal<number>(new Date().getFullYear());
  anioLectivo  = signal<number>(1);
  comision     = signal('');
  cupo         = signal<number>(30);
  preceptorId  = signal<number | null>(null);

  preceptores = signal<Preceptor[]>([]);
  guardando   = signal(false);
  error       = signal<string | null>(null);

  constructor(
    private cursosService: CursosService,
    private preceptoresService: PreceptoresService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.preceptoresService.listar().subscribe({
      next: data => this.preceptores.set(data.filter(p => p.activo)),
      error: () => this.error.set('Error al cargar preceptores.')
    });
  }

  guardar(): void {
    if (!this.comision() || !this.preceptorId() || this.cupo() <= 0) {
      this.error.set('Todos los campos son obligatorios y el cupo debe ser mayor a 0.');
      return;
    }
    this.guardando.set(true);
    this.error.set(null);
    this.cursosService.crear({
      anio: this.anio(),
      anioLectivo: this.anioLectivo(),
      comision: this.comision(),
      cupo: this.cupo(),
      preceptorId: this.preceptorId()!
    }).subscribe({
      next: () => this.router.navigate(['/cursos']),
      error: (e) => { this.error.set(e.error?.mensaje ?? 'Error al crear el curso.'); this.guardando.set(false); }
    });
  }

  cancelar(): void { this.router.navigate(['/cursos']); }
}
