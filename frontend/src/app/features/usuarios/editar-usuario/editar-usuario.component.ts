import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UsuariosService } from '../usuarios.service';

@Component({
  selector: 'app-editar-usuario',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './editar-usuario.component.html',
  styleUrl: './editar-usuario.component.scss'
})
export class EditarUsuarioComponent implements OnInit {
  form: FormGroup;
  cargando = signal(false);
  cargandoDatos = signal(true);
  error = signal<string | null>(null);
  usuarioId!: number;

  readonly roles = ['Estudiante', 'Docente', 'Preceptor', 'Direccion'];

  constructor(
    private fb: FormBuilder,
    private usuariosService: UsuariosService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      apellido: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      rol: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.usuarioId = Number(this.route.snapshot.paramMap.get('id'));

    this.usuariosService.listar().subscribe({
      next: (usuarios) => {
        const usuario = usuarios.find(u => u.id === this.usuarioId);
        if (!usuario) {
          this.error.set('Usuario no encontrado.');
          this.cargandoDatos.set(false);
          return;
        }
        this.form.patchValue({
          nombre: usuario.nombre,
          apellido: usuario.apellido,
          email: usuario.email,
          rol: usuario.rol
        });
        this.cargandoDatos.set(false);
      },
      error: () => {
        this.error.set('Error al cargar los datos del usuario.');
        this.cargandoDatos.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid || this.cargando()) return;

    this.error.set(null);
    this.cargando.set(true);

    this.usuariosService.modificar(this.usuarioId, this.form.value).subscribe({
      next: () => {
        this.cargando.set(false);
        this.router.navigate(['/usuarios']);
      },
      error: (err) => {
        this.cargando.set(false);
        this.error.set(err?.error?.detail ?? 'Error al modificar el usuario.');
      }
    });
  }

  volver(): void {
    this.router.navigate(['/usuarios']);
  }
}
