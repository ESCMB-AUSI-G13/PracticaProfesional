import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PreceptoresService } from '../preceptores.service';

@Component({
  selector: 'app-editar-preceptor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './editar-preceptor.component.html',
  styleUrl: './editar-preceptor.component.scss'
})
export class EditarPreceptorComponent implements OnInit {
  form: FormGroup;
  cargando = signal(false);
  cargandoDatos = signal(true);
  error = signal<string | null>(null);
  usuarioId!: number;

  readonly turnos = ['Mañana', 'Tarde', 'Noche', 'Vespertino'];

  constructor(
    private fb: FormBuilder,
    private preceptoresService: PreceptoresService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      apellido: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      telefono: ['', [Validators.required, Validators.maxLength(20)]],
      turno: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.usuarioId = Number(this.route.snapshot.paramMap.get('id'));

    this.preceptoresService.listar().subscribe({
      next: (preceptores) => {
        const preceptor = preceptores.find(p => p.usuarioId === this.usuarioId);
        if (!preceptor) {
          this.error.set('Preceptor no encontrado.');
          this.cargandoDatos.set(false);
          return;
        }
        this.form.patchValue({
          nombre: preceptor.nombre,
          apellido: preceptor.apellido,
          email: preceptor.email,
          telefono: preceptor.telefono,
          turno: preceptor.turno
        });
        this.cargandoDatos.set(false);
      },
      error: () => {
        this.error.set('Error al cargar los datos del preceptor.');
        this.cargandoDatos.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid || this.cargando()) return;

    this.error.set(null);
    this.cargando.set(true);

    this.preceptoresService.modificar(this.usuarioId, this.form.value).subscribe({
      next: () => {
        this.cargando.set(false);
        this.router.navigate(['/preceptores']);
      },
      error: (err) => {
        this.cargando.set(false);
        this.error.set(err?.error?.detail ?? 'Error al modificar el preceptor.');
      }
    });
  }

  volver(): void {
    this.router.navigate(['/preceptores']);
  }
}
