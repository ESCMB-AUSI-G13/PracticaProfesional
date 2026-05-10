import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { environment } from '../../../../environments/environment';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CarrerasService, Carrera } from '../../carreras/carreras.service';

@Component({
  selector: 'app-registro',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './registro.component.html',
  styleUrl: './registro.component.scss'
})
export class RegistroComponent implements OnInit {
  form: FormGroup;
  cargando = signal(false);
  error = signal<string | null>(null);
  exitoso = signal(false);

  readonly anios = [1, 2, 3, 4, 5, 6];
  carreras = signal<Carrera[]>([]);

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private router: Router,
    private carrerasService: CarrerasService
  ) {
    this.form = this.fb.group({
      dni: ['', [Validators.required, Validators.pattern(/^\d{7,8}$/)]],
      legajo: ['', [Validators.required, Validators.maxLength(20)]],
      email: ['', [Validators.required, Validators.email]],
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      apellido: ['', [Validators.required, Validators.maxLength(100)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      carreraId: [null, Validators.required],
      anio: [1, [Validators.required, Validators.min(1), Validators.max(6)]],
      fechaDeIngreso: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.carrerasService.listar().subscribe({
      next: data => this.carreras.set(data),
      error: () => this.error.set('Error al cargar las carreras.')
    });
  }

  onSubmit(): void {
    if (this.form.invalid || this.cargando()) return;

    this.error.set(null);
    this.cargando.set(true);

    const payload = {
      ...this.form.value,
      anio: Number(this.form.value.anio),
      carreraId: Number(this.form.value.carreraId),
      fechaDeIngreso: new Date(this.form.value.fechaDeIngreso).toISOString()
    };

    this.http.post(`${environment.apiUrl}/auth/registro`, payload).subscribe({
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
