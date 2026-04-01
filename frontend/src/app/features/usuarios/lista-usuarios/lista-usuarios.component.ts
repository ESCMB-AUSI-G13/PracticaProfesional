import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UsuariosService, Usuario } from '../usuarios.service';
import { AuthService } from '../../auth/services/auth.service';

@Component({
  selector: 'app-lista-usuarios',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './lista-usuarios.component.html',
  styleUrl: './lista-usuarios.component.scss'
})
export class ListaUsuariosComponent implements OnInit {
  usuarios = signal<Usuario[]>([]);
  cargando = signal(true);
  error = signal<string | null>(null);

  readonly roles = ['Estudiante', 'Docente', 'Preceptor', 'Direccion'];

  constructor(
    private usuariosService: UsuariosService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargarUsuarios();
  }

  cargarUsuarios(rol?: string): void {
    this.cargando.set(true);
    this.error.set(null);

    this.usuariosService.listar(rol).subscribe({
      next: (data) => {
        this.usuarios.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('Error al cargar usuarios.');
        this.cargando.set(false);
      }
    });
  }

  filtrarPorRol(event: Event): void {
    const rol = (event.target as HTMLSelectElement).value;
    this.cargarUsuarios(rol || undefined);
  }

  desactivar(id: number): void {
    if (!confirm('¿Desactivar este usuario?')) return;

    this.usuariosService.desactivar(id).subscribe({
      next: () => this.cargarUsuarios(),
      error: () => this.error.set('Error al desactivar el usuario.')
    });
  }

  irACrear(): void {
    this.router.navigate(['/usuarios/nuevo']);
  }

  irAEditar(id: number): void {
    this.router.navigate(['/usuarios', id, 'editar']);
  }

  logout(): void {
    this.authService.logout();
  }
}
