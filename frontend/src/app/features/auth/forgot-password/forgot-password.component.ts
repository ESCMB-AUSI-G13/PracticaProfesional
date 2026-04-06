import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss'
})
export class ForgotPasswordComponent {
  form: FormGroup;
  cargando = signal(false);
  error = signal<string | null>(null);
  enviado = signal(false);
  enlaceDevMode = signal<string | null>(null);

  constructor(private fb: FormBuilder, private authService: AuthService) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  get emailControl() { return this.form.get('email')!; }

  onSubmit(): void {
    if (this.form.invalid || this.cargando()) return;

    this.error.set(null);
    this.cargando.set(true);

    this.authService.solicitarRestablecimiento(this.emailControl.value).subscribe({
      next: (res) => {
        this.cargando.set(false);
        this.enviado.set(true);
        if (res.enlace) this.enlaceDevMode.set(res.enlace);
      },
      error: (err) => {
        this.cargando.set(false);
        const mensaje = err?.error?.detail ?? 'Ocurrió un error. Intentá nuevamente.';
        this.error.set(mensaje);
      }
    });
  }
}
