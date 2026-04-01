import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { EstudiantesService } from '../estudiantes.service';

@Component({
  selector: 'app-crear-estudiante',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './crear-estudiante.component.html',
  styleUrl: './crear-estudiante.component.scss'
})
export class CrearEstudianteComponent {
  form: FormGroup;
  cargando = signal(false);
  error = signal<string | null>(null);

  readonly anios = [1, 2, 3, 4, 5, 6];

  constructor(
    private fb: FormBuilder,
    private estudiantesService: EstudiantesService,
    private router: Router
  ) {
    this.form = this.fb.group({
      dni: ['', [Validators.required, Validators.pattern(/^\d{7,8}$/)]],
      legajo: ['', [Validators.required, Validators.maxLength(20)]],
      email: ['', [Validators.required, Validators.email]],
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      apellido: ['', [Validators.required, Validators.maxLength(100)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      anio: [1, [Validators.required, Validators.min(1), Validators.max(6)]],
      fechaDeIngreso: ['', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.form.invalid || this.cargando()) return;

    this.error.set(null);
    this.cargando.set(true);

    const payload = {
      ...this.form.value,
      anio: Number(this.form.value.anio),
      fechaDeIngreso: new Date(this.form.value.fechaDeIngreso).toISOString()
    };

    this.estudiantesService.crear(payload).subscribe({
      next: () => {
        this.cargando.set(false);
        this.router.navigate(['/estudiantes']);
      },
      error: (err) => {
        this.cargando.set(false);
        this.error.set(err?.error?.detail ?? 'Error al crear el estudiante.');
      }
    });
  }

  volver(): void {
    this.router.navigate(['/estudiantes']);
  }
}
