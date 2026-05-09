import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-registro',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './registro.component.html',
  styleUrl: './registro.component.scss'
})
export class RegistroComponent {
  form: FormGroup;
  cargando = signal(false);
  error = signal<string | null>(null);
  exitoso = signal(false);

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private router: Router
  ) {
    this.form = this.fb.group({
      dni: ['', [Validators.required, Validators.pattern(/^\d{7,8}$/)]],
      legajo: ['', [Validators.required, Validators.maxLength(20)]],
      email: ['', [Validators.required, Validators.email]],
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      apellido: ['', [Validators.required, Validators.maxLength(100)]],
      password: ['', [Validators.required, Validators.minLength(8)]]
    });
  }

  onSubmit(): void {
    if (this.form.invalid || this.cargando()) return;

    this.error.set(null);
    this.cargando.set(true);

    this.http.post('http://localhost:5000/api/auth/registro', this.form.value).subscribe({
      next: () => {
        this.cargando.set(false);
        this.exitoso.set(true);
      },
      error: (err) => {
        this.cargando.set(false);
        this.error.set(err?.error?.detail ?? 'Error al registrarse. Intente nuevamente.');
      }
    });
  }

  irAlLogin(): void {
    this.router.navigate(['/login']);
  }
}
