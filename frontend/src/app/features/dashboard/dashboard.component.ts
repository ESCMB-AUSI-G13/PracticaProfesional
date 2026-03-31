import { Component, inject } from '@angular/core';
import { AuthService } from '../auth/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  template: `
    <div style="padding: 2rem; font-family: sans-serif;">
      <h1>Bienvenido, {{ authService.usuario()?.nombreCompleto }}</h1>
      <p>Rol: <strong>{{ authService.usuario()?.rol }}</strong></p>
      <button (click)="authService.logout()" style="margin-top: 1rem; padding: 0.5rem 1rem; cursor: pointer;">
        Cerrar sesión
      </button>
    </div>
  `
})
export class DashboardComponent {
  authService = inject(AuthService);
}
