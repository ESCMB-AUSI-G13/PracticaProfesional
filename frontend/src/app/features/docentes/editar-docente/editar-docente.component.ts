import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { DocentesService } from '../docentes.service';

@Component({
  selector: 'app-editar-docente',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './editar-docente.component.html',
  styleUrl: './editar-docente.component.scss'
})
export class EditarDocenteComponent implements OnInit {
  form: FormGroup;
  cargando = signal(false);
  cargandoDatos = signal(true);
  error = signal<string | null>(null);
  usuarioId!: number;

  readonly categorias = ['Titular', 'Adjunto', 'Interino', 'Suplente', 'Reemplazante'];

  constructor(
    private fb: FormBuilder,
    private docentesService: DocentesService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      apellido: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      telefono: ['', [Validators.required, Validators.maxLength(20)]],
      categoria: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.usuarioId = Number(this.route.snapshot.paramMap.get('id'));

    this.docentesService.listar().subscribe({
      next: (docentes) => {
        const docente = docentes.find(d => d.usuarioId === this.usuarioId);
        if (!docente) {
          this.error.set('Docente no encontrado.');
          this.cargandoDatos.set(false);
          return;
        }
        this.form.patchValue({
          nombre: docente.nombre,
          apellido: docente.apellido,
          email: docente.email,
          telefono: docente.telefono,
          categoria: docente.categoria
        });
        this.cargandoDatos.set(false);
      },
      error: () => {
        this.error.set('Error al cargar los datos del docente.');
        this.cargandoDatos.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid || this.cargando()) return;

    this.error.set(null);
    this.cargando.set(true);

    this.docentesService.modificar(this.usuarioId, this.form.value).subscribe({
      next: () => {
        this.cargando.set(false);
        this.router.navigate(['/docentes']);
      },
      error: (err) => {
        this.cargando.set(false);
        this.error.set(err?.error?.detail ?? 'Error al modificar el docente.');
      }
    });
  }

  volver(): void {
    this.router.navigate(['/docentes']);
  }
}
