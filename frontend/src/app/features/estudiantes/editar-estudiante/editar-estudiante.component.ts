import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EstudiantesService } from '../estudiantes.service';

@Component({
  selector: 'app-editar-estudiante',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './editar-estudiante.component.html',
  styleUrl: './editar-estudiante.component.scss'
})
export class EditarEstudianteComponent implements OnInit {
  form: FormGroup;
  cargando = signal(false);
  cargandoDatos = signal(true);
  error = signal<string | null>(null);
  usuarioId!: number;

  readonly anios = [1, 2, 3, 4, 5, 6];
  readonly condiciones = ['Regular', 'Libre', 'Promocional', 'Egresado', 'Desertor'];
  readonly carreras = [
    { valor: 'Profesorado', etiqueta: 'Prof. de Educación Secundaria en Economía' },
    { valor: 'Trayecto',    etiqueta: 'Trayecto Pedagógico para Graduados No Docentes' }
  ];

  constructor(
    private fb: FormBuilder,
    private estudiantesService: EstudiantesService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      apellido: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      anio: [1, [Validators.required, Validators.min(1), Validators.max(6)]],
      plan: ['', Validators.required],
      condicion: ['Regular', Validators.required]
    });
  }

  ngOnInit(): void {
    this.usuarioId = Number(this.route.snapshot.paramMap.get('id'));

    this.estudiantesService.listar().subscribe({
      next: (estudiantes) => {
        const estudiante = estudiantes.find(e => e.usuarioId === this.usuarioId);
        if (!estudiante) {
          this.error.set('Estudiante no encontrado.');
          this.cargandoDatos.set(false);
          return;
        }
        this.form.patchValue({
          nombre: estudiante.nombre,
          apellido: estudiante.apellido,
          email: estudiante.email,
          anio: estudiante.anio,
          plan: estudiante.plan,
          condicion: estudiante.condicion
        });
        this.cargandoDatos.set(false);
      },
      error: () => {
        this.error.set('Error al cargar los datos del estudiante.');
        this.cargandoDatos.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid || this.cargando()) return;

    this.error.set(null);
    this.cargando.set(true);

    const payload = { ...this.form.value, anio: Number(this.form.value.anio) };

    this.estudiantesService.modificar(this.usuarioId, payload).subscribe({
      next: () => {
        this.cargando.set(false);
        this.router.navigate(['/estudiantes']);
      },
      error: (err) => {
        this.cargando.set(false);
        this.error.set(err?.error?.detail ?? 'Error al modificar el estudiante.');
      }
    });
  }

  volver(): void {
    this.router.navigate(['/estudiantes']);
  }
}
