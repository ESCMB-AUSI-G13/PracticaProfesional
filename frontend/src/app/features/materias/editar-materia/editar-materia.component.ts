import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MateriasService } from '../materias.service';

@Component({
  selector: 'app-editar-materia',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './editar-materia.component.html',
  styleUrl: './editar-materia.component.scss'
})
export class EditarMateriaComponent implements OnInit {
  id      = 0;
  codigo  = signal('');
  nombre  = signal('');
  plan    = signal('');
  cargando  = signal(true);
  guardando = signal(false);
  error     = signal<string | null>(null);

  constructor(
    private materiasService: MateriasService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    this.materiasService.listar().subscribe({
      next: data => {
        const m = data.find(x => x.id === this.id);
        if (!m) { this.router.navigate(['/materias']); return; }
        this.codigo.set(m.codigo);
        this.nombre.set(m.nombre);
        this.plan.set(m.plan);
        this.cargando.set(false);
      },
      error: () => { this.error.set('Error al cargar la materia.'); this.cargando.set(false); }
    });
  }

  guardar(): void {
    if (!this.nombre() || !this.plan()) { this.error.set('Todos los campos son obligatorios.'); return; }
    this.guardando.set(true);
    this.error.set(null);
    this.materiasService.modificar(this.id, { nombre: this.nombre(), plan: this.plan() }).subscribe({
      next: () => this.router.navigate(['/materias']),
      error: (e) => { this.error.set(e.error?.mensaje ?? 'Error al guardar.'); this.guardando.set(false); }
    });
  }

  cancelar(): void { this.router.navigate(['/materias']); }
}
