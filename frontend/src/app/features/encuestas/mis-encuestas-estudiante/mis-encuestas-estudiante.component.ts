import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EncuestasService, EncuestaDto } from '../encuestas.service';
import { ModalEncuestaComponent } from '../modal-encuesta/modal-encuesta.component';

@Component({
  selector: 'app-mis-encuestas-estudiante',
  standalone: true,
  imports: [CommonModule, ModalEncuestaComponent],
  templateUrl: './mis-encuestas-estudiante.component.html',
  styleUrl: './mis-encuestas-estudiante.component.scss'
})
export class MisEncuestasEstudianteComponent implements OnInit {
  encuestaPendiente = signal<EncuestaDto | null>(null);
  cargando          = signal(true);
  error             = signal<string | null>(null);
  respondiendo      = signal(false);

  constructor(private encuestasService: EncuestasService) {}

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.encuestasService.obtenerPendiente().subscribe({
      next: (encuesta) => {
        this.encuestaPendiente.set(encuesta);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('No se pudo verificar si tenés encuestas pendientes.');
        this.cargando.set(false);
      }
    });
  }

  responderAhora(): void {
    this.respondiendo.set(true);
  }

  onEncuestaCompletada(): void {
    this.respondiendo.set(false);
    this.cargar();
  }

  onCerrarModal(): void {
    this.respondiendo.set(false);
  }
}
