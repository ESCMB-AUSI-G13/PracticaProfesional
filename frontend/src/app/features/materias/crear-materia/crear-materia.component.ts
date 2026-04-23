import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MateriasService } from '../materias.service';

@Component({
  selector: 'app-crear-materia',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './crear-materia.component.html',
  styleUrl: './crear-materia.component.scss'
})
export class CrearMateriaComponent {
  codigo  = signal('');
  nombre  = signal('');
  plan    = signal('');
  guardando = signal(false);
  error     = signal<string | null>(null);

  constructor(private materiasService: MateriasService, private router: Router) {}

  guardar(): void {
    if (!this.codigo() || !this.nombre() || !this.plan()) {
      this.error.set('Todos los campos son obligatorios.');
      return;
    }
    this.guardando.set(true);
    this.error.set(null);
    this.materiasService.crear({ codigo: this.codigo(), nombre: this.nombre(), plan: this.plan() }).subscribe({
      next: () => this.router.navigate(['/materias']),
      error: (e) => {
        this.error.set(e.error?.mensaje ?? 'Error al crear la materia.');
        this.guardando.set(false);
      }
    });
  }

  cancelar(): void { this.router.navigate(['/materias']); }
}
