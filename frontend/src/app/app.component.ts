import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BannerVistaRolComponent } from './shared/banner-vista-rol/banner-vista-rol.component';
import { AuthService } from './features/auth/services/auth.service';
import { SesionService } from './features/sesiones/sesion.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, BannerVistaRolComponent],
  template: `
    <app-banner-vista-rol />
    <router-outlet />
  `
})
export class AppComponent implements OnInit {
  private authService  = inject(AuthService);
  private sesionService = inject(SesionService);

  ngOnInit(): void {
    // Si hay sesión guardada (recarga de página), retomar el heartbeat
    if (this.authService.estaAutenticado()) {
      this.sesionService.iniciarHeartbeat();
    }
  }
}
