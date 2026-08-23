import { Component, inject, signal, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../features/auth/services/auth.service';
import { AsistenteIAService } from '../../features/asistente-ia/asistente-ia.service';
import { MensajeChat } from '../../features/asistente-ia/models/mensaje-chat.model';

interface PreguntaSugerida {
  label: string;
  pregunta: string;
}

const PREGUNTAS_SUGERIDAS: PreguntaSugerida[] = [
  { label: 'Tasa de deserción general',        pregunta: '¿Cuál es la tasa de deserción general de la institución?' },
  { label: 'Estudiantes en riesgo alto',        pregunta: '¿Cuántos estudiantes están en riesgo académico alto?' },
  { label: 'Carrera con más egresados',         pregunta: '¿Qué carrera tiene más egresados este año?' },
  { label: 'Retención por cohorte',             pregunta: '¿Cómo viene la retención por cohorte este año?' },
  { label: 'Inasistencias del mes',             pregunta: '¿Cómo está el nivel de inasistencias este mes?' },
  { label: 'Evolución de notas',                pregunta: '¿Cómo evolucionaron los promedios de notas este año?' },
  { label: 'Comisiones con menor aprobación',   pregunta: '¿Qué comisiones tienen menor porcentaje de aprobación?' },
  { label: 'Última encuesta de satisfacción',   pregunta: '¿Cómo resultó la última encuesta de satisfacción institucional?' },
];

@Component({
  selector: 'app-asistente-ia-fab',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './asistente-ia-fab.component.html',
  styleUrl: './asistente-ia-fab.component.scss'
})
export class AsistenteIaFabComponent implements AfterViewChecked {
  private readonly authService = inject(AuthService);
  private readonly asistenteSvc = inject(AsistenteIAService);

  @ViewChild('listaMensajes') private listaMensajesRef?: ElementRef<HTMLDivElement>;
  private debeHacerScroll = false;

  readonly abierto = signal(false);
  readonly enviando = signal(false);
  readonly mensajes = signal<MensajeChat[]>([]);
  readonly preguntasSugeridas = PREGUNTAS_SUGERIDAS;

  textoInput = '';

  get mostrarFab(): boolean {
    return this.authService.rolVista() === 'Direccion';
  }

  togglePanel(): void {
    this.abierto.update(v => !v);
  }

  enviarDesdeInput(): void {
    const texto = this.textoInput.trim();
    if (!texto) return;
    this.textoInput = '';
    this.enviarPregunta(texto);
  }

  enviarPregunta(texto: string): void {
    if (this.enviando()) return;

    this.mensajes.update(ms => [...ms, { rol: 'usuario', texto, timestamp: new Date() }]);
    this.enviando.set(true);
    this.debeHacerScroll = true;

    this.asistenteSvc.preguntar(texto).subscribe({
      next: respuesta => {
        this.mensajes.update(ms => [...ms, { rol: 'asistente', texto: respuesta.respuesta, timestamp: new Date() }]);
        this.enviando.set(false);
        this.debeHacerScroll = true;
      },
      error: (err: HttpErrorResponse) => {
        const mensaje = err.status === 503
          ? 'El asistente no está disponible en este momento, probá más tarde o consultá los reportes directamente.'
          : 'No se pudo procesar tu consulta. Probá reformularla.';
        this.mensajes.update(ms => [...ms, { rol: 'error', texto: mensaje, timestamp: new Date() }]);
        this.enviando.set(false);
        this.debeHacerScroll = true;
      }
    });
  }

  ngAfterViewChecked(): void {
    if (this.debeHacerScroll && this.listaMensajesRef) {
      this.listaMensajesRef.nativeElement.scrollTop = this.listaMensajesRef.nativeElement.scrollHeight;
      this.debeHacerScroll = false;
    }
  }
}
