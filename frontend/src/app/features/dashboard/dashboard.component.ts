import { Component, inject, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../auth/services/auth.service';

interface Modulo {
  titulo: string;
  descripcion: string;
  ruta: string;
  roles: string[];
}

const MODULOS: Modulo[] = [
  {
    titulo: 'Gestión de Usuarios',
    descripcion: 'Alta, baja y modificación de docentes, preceptores y estudiantes.',
    ruta: '/usuarios',
    roles: ['Direccion']
  }
];

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {
  authService = inject(AuthService);

  // Filtra módulos por rolVista (no por rol real)
  modulosVisibles = computed(() => {
    const rol = this.authService.rolVista();
    if (!rol) return [];
    return MODULOS.filter(m => m.roles.includes(rol));
  });

  activarVista(event: Event): void {
    const rol = (event.target as HTMLSelectElement).value;
    if (rol) this.authService.activarVista(rol);
  }
}
