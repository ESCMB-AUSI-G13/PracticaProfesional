import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MateriasService } from '../materias.service';
import { CarrerasService, Carrera } from '../../carreras/carreras.service';

@Component({
  selector: 'app-crear-materia',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './crear-materia.component.html',
  styleUrl: './crear-materia.component.scss'
})
export class CrearMateriaComponent implements OnInit {
  nombre    = signal('');
  carreraId = signal<number | null>(null);
  guardando = signal(false);
  error     = signal<string | null>(null);

  carreras = signal<Carrera[]>([]);

  constructor(
    private materiasService: MateriasService,
    private carrerasService: CarrerasService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.carrerasService.listar().subscribe({
      next: data => this.carreras.set(data),
      error: () => this.error.set('Error al cargar las carreras.')
    });
  }

  guardar(): void {
    if (!this.nombre() || !this.carreraId()) {
      this.error.set('Todos los campos son obligatorios.');
      return;
    }
    this.guardando.set(true);
    this.error.set(null);
    this.materiasService.crear({ nombre: this.nombre(), carreraId: this.carreraId()! }).subscribe({
      next: () => this.router.navigate(['/materias']),
      error: (e) => {
        this.error.set(e.error?.detail ?? 'Error al crear la materia.');
        this.guardando.set(false);
      }
    });
  }

  cancelar(): void { this.router.navigate(['/materias']); }
}
