import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UsuariosService, Usuario } from '../usuarios.service';
import { AuthService } from '../../auth/services/auth.service';
import { SesionService } from '../../sesiones/sesion.service';

const POLL_SESIONES_MS = 30_000;

@Component({
  selector: 'app-lista-usuarios',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './lista-usuarios.component.html',
  styleUrl: './lista-usuarios.component.scss'
})
export class ListaUsuariosComponent implements OnInit, OnDestroy {
  usuarios       = signal<Usuario[]>([]);
  cargando       = signal(true);
  error          = signal<string | null>(null);
  sesionesActivas = signal<Set<number>>(new Set());

  readonly roles = ['Estudiante', 'Docente', 'Preceptor', 'Direccion'];

  private pollIntervalo: ReturnType<typeof setInterval> | null = null;

  constructor(
    private usuariosService: UsuariosService,
    private authService: AuthService,
    private sesionService: SesionService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargarUsuarios();
    this.cargarSesiones();
    // Actualiza el indicador de presencia cada 30 s
    this.pollIntervalo = setInterval(() => this.cargarSesiones(), POLL_SESIONES_MS);
  }

  ngOnDestroy(): void {
    if (this.pollIntervalo !== null) clearInterval(this.pollIntervalo);
  }

  cargarUsuarios(rol?: string): void {
    this.cargando.set(true);
    this.error.set(null);
    this.usuariosService.listar(rol).subscribe({
      next: (data) => { this.usuarios.set(data); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar usuarios.'); this.cargando.set(false); }
    });
  }

  cargarSesiones(): void {
    this.sesionService.obtenerActivas().subscribe({
      next: (ids) => this.sesionesActivas.set(new Set(ids)),
      error: () => {}
    });
  }

  estaActivo(id: number): boolean {
    return this.sesionesActivas().has(id);
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

  reactivar(id: number): void {
    if (!confirm('¿Reactivar este usuario?')) return;
    this.usuariosService.reactivar(id).subscribe({
      next: () => this.cargarUsuarios(),
      error: () => this.error.set('Error al reactivar el usuario.')
    });
  }

  irAlDashboard(): void { this.router.navigate(['/dashboard']); }
  irACrear(): void       { this.router.navigate(['/usuarios/nuevo']); }
  irAEditar(id: number): void { this.router.navigate(['/usuarios', id, 'editar']); }
  logout(): void         { this.authService.logout(); }
}
