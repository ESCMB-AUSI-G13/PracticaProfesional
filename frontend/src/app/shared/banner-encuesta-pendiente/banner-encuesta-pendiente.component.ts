import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../features/auth/services/auth.service';
import { EncuestasService } from '../../features/encuestas/encuestas.service';

@Component({
  selector: 'app-banner-encuesta-pendiente',
  standalone: true,
  templateUrl: './banner-encuesta-pendiente.component.html',
  styleUrl: './banner-encuesta-pendiente.component.scss'
})
export class BannerEncuestaPendienteComponent implements OnInit {
  private authService = inject(AuthService);
  private encuestasService = inject(EncuestasService);
  private router = inject(Router);

  hayPendiente = signal(false);
  descartado   = signal(false);

  ngOnInit(): void {
    if (this.authService.rolVista() !== 'Estudiante') return;

    this.encuestasService.obtenerPendiente().subscribe({
      next: (encuesta) => this.hayPendiente.set(!!encuesta),
      error: () => {}
    });
  }

  mostrar(): boolean {
    return this.hayPendiente() && !this.descartado();
  }

  ir(): void {
    this.router.navigate(['/mis-encuestas-pendientes']);
  }

  descartar(): void {
    this.descartado.set(true);
  }
}
