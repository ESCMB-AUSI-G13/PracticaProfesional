import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CursosService } from '../cursos.service';

@Component({
  selector: 'app-editar-curso',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './editar-curso.component.html',
  styleUrl: './editar-curso.component.scss'
})
export class EditarCursoComponent implements OnInit {
  id       = 0;
  anio     = signal(0);
  comision = signal('');
  cupo     = signal(0);
  preceptorNombre = signal('');

  cargando  = signal(true);
  guardando = signal(false);
  error     = signal<string | null>(null);

  constructor(
    private cursosService: CursosService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    this.cursosService.listar().subscribe({
      next: data => {
        const c = data.find(x => x.id === this.id);
        if (!c) { this.router.navigate(['/cursos']); return; }
        this.anio.set(c.anio);
        this.comision.set(c.comision);
        this.cupo.set(c.cupo);
        this.preceptorNombre.set(c.preceptorNombre);
        this.cargando.set(false);
      },
      error: () => { this.error.set('Error al cargar el curso.'); this.cargando.set(false); }
    });
  }

  guardar(): void {
    if (!this.comision() || this.cupo() <= 0) { this.error.set('Comisión y cupo son obligatorios.'); return; }
    this.guardando.set(true);
    this.error.set(null);
    this.cursosService.modificar(this.id, { comision: this.comision(), cupo: this.cupo() }).subscribe({
      next: () => this.router.navigate(['/cursos']),
      error: (e) => { this.error.set(e.error?.mensaje ?? 'Error al guardar.'); this.guardando.set(false); }
    });
  }

  cancelar(): void { this.router.navigate(['/cursos']); }
}
