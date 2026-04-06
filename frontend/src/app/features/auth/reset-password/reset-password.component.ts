import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

function passwordsIguales(control: AbstractControl): ValidationErrors | null {
  const nueva = control.get('nuevaPassword')?.value;
  const confirmar = control.get('confirmarPassword')?.value;
  return nueva && confirmar && nueva !== confirmar ? { noCoinciden: true } : null;
}

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss'
})
export class ResetPasswordComponent implements OnInit {
  form: FormGroup;
  cargando = signal(false);
  error = signal<string | null>(null);
  exitoso = signal(false);
  tokenInvalido = signal(false);
  mostrarPassword = signal(false);

  private token = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      nuevaPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmarPassword: ['', Validators.required]
    }, { validators: passwordsIguales });
  }

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    if (!this.token) this.tokenInvalido.set(true);
  }

  get nuevaPasswordControl() { return this.form.get('nuevaPassword')!; }
  get confirmarPasswordControl() { return this.form.get('confirmarPassword')!; }

  togglePassword(): void {
    this.mostrarPassword.update(v => !v);
  }

  onSubmit(): void {
    if (this.form.invalid || this.cargando() || this.tokenInvalido()) return;

    this.error.set(null);
    this.cargando.set(true);

    this.authService.restablecerPassword(
      this.token,
      this.nuevaPasswordControl.value,
      this.confirmarPasswordControl.value
    ).subscribe({
      next: () => {
        this.cargando.set(false);
        this.exitoso.set(true);
        setTimeout(() => this.router.navigate(['/login']), 3000);
      },
      error: (err) => {
        this.cargando.set(false);
        const mensaje = err?.error?.detail ?? 'El enlace es inválido o ha expirado.';
        this.error.set(mensaje);
      }
    });
  }
}
