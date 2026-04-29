import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './features/auth/services/auth.service';
import { SesionService } from './features/sesiones/sesion.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `<router-outlet />`
})
export class AppComponent implements OnInit {
  private authService  = inject(AuthService);
  private sesionService = inject(SesionService);

  ngOnInit(): void {
    if (this.authService.estaAutenticado()) {
      this.sesionService.iniciarHeartbeat();
    }
  }
}
