import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UsuariosService } from '../usuarios.service';

@Component({
  selector: 'app-crear-usuario',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './crear-usuario.component.html',
  styleUrl: './crear-usuario.component.scss'
})
export class CrearUsuarioComponent {
  form: FormGroup;
  cargando = signal(false);
  error = signal<string | null>(null);

  readonly roles = ['Estudiante', 'Docente', 'Preceptor', 'Direccion'];

  constructor(
    private fb: FormBuilder,
    private usuariosService: UsuariosService,
    private router: Router
  ) {
    this.form = this.fb.group({
      dni: ['', [Validators.required, Validators.pattern(/^\d{7,8}$/)]],
      email: ['', [Validators.required, Validators.email]],
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      apellido: ['', [Validators.required, Validators.maxLength(100)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      rol: ['Estudiante', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.form.invalid || this.cargando()) return;

    this.error.set(null);
    this.cargando.set(true);

    this.usuariosService.crear(this.form.value).subscribe({
      next: () => {
        this.cargando.set(false);
        this.router.navigate(['/usuarios']);
      },
      error: (err) => {
        this.cargando.set(false);
        this.error.set(err?.error?.detail ?? 'Error al crear el usuario.');
      }
    });
  }

  volver(): void {
    this.router.navigate(['/usuarios']);
  }
}
