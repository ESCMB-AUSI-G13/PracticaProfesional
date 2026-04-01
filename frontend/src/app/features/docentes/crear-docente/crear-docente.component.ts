import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { DocentesService } from '../docentes.service';

@Component({
  selector: 'app-crear-docente',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './crear-docente.component.html',
  styleUrl: './crear-docente.component.scss'
})
export class CrearDocenteComponent {
  form: FormGroup;
  cargando = signal(false);
  error = signal<string | null>(null);

  readonly categorias = ['Titular', 'Adjunto', 'Interino', 'Suplente', 'Reemplazante'];

  constructor(
    private fb: FormBuilder,
    private docentesService: DocentesService,
    private router: Router
  ) {
    this.form = this.fb.group({
      dni: ['', [Validators.required, Validators.pattern(/^\d{7,8}$/)]],
      legajo: ['', [Validators.required, Validators.maxLength(20)]],
      email: ['', [Validators.required, Validators.email]],
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      apellido: ['', [Validators.required, Validators.maxLength(100)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      telefono: ['', [Validators.required, Validators.maxLength(20)]],
      categoria: ['Titular', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.form.invalid || this.cargando()) return;

    this.error.set(null);
    this.cargando.set(true);

    this.docentesService.crear(this.form.value).subscribe({
      next: () => {
        this.cargando.set(false);
        this.router.navigate(['/docentes']);
      },
      error: (err) => {
        this.cargando.set(false);
        this.error.set(err?.error?.detail ?? 'Error al crear el docente.');
      }
    });
  }

  volver(): void {
    this.router.navigate(['/docentes']);
  }
}
