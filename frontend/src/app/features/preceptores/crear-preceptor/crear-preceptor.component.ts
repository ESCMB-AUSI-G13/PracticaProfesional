import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { PreceptoresService } from '../preceptores.service';

@Component({
  selector: 'app-crear-preceptor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './crear-preceptor.component.html',
  styleUrl: './crear-preceptor.component.scss'
})
export class CrearPreceptorComponent {
  form: FormGroup;
  cargando = signal(false);
  error = signal<string | null>(null);

  readonly turnos = ['Mañana', 'Tarde', 'Noche', 'Vespertino'];

  constructor(
    private fb: FormBuilder,
    private preceptoresService: PreceptoresService,
    private router: Router
  ) {
    this.form = this.fb.group({
      dni: ['', [Validators.required, Validators.pattern(/^\d{7,8}$/)]],
      email: ['', [Validators.required, Validators.email]],
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      apellido: ['', [Validators.required, Validators.maxLength(100)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      telefono: ['', [Validators.required, Validators.maxLength(20)]],
      turno: ['Mañana', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.form.invalid || this.cargando()) return;

    this.error.set(null);
    this.cargando.set(true);

    this.preceptoresService.crear(this.form.value).subscribe({
      next: () => {
        this.cargando.set(false);
        this.router.navigate(['/preceptores']);
      },
      error: (err) => {
        this.cargando.set(false);
        this.error.set(err?.error?.detail ?? 'Error al crear el preceptor.');
      }
    });
  }

  volver(): void {
    this.router.navigate(['/preceptores']);
  }
}
