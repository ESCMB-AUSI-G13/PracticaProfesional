import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  EncuestasService, EncuestaDto, CrearEncuestaRequest, AgregarPreguntaRequest
} from '../encuestas.service';

@Component({
  selector: 'app-panel-encuestas',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-encuestas.component.html',
  styleUrl: './panel-encuestas.component.scss'
})
export class PanelEncuestasComponent implements OnInit {
  encuestas    = signal<EncuestaDto[]>([]);
  cargando     = signal(true);
  error        = signal<string | null>(null);
  exito        = signal<string | null>(null);

  // Panel crear encuesta
  mostrarFormEncuesta = signal(false);
  guardandoEncuesta   = signal(false);
  nuevaEncuesta: CrearEncuestaRequest = {
    titulo: '', descripcion: null, tipo: 'SatisfaccionGeneral', cicloLectivo: new Date().getFullYear(), materiaId: null
  };

  // Panel agregar pregunta
  encuestaSeleccionadaId = signal<number | null>(null);
  mostrarFormPregunta    = signal(false);
  guardandoPregunta      = signal(false);
  nuevaPregunta: AgregarPreguntaRequest = {
    encuestaId: 0, texto: '', orden: 1, tipoPregunta: 'EscalaLikert', esObligatoria: true
  };

  constructor(private service: EncuestasService) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.cargando.set(true);
    this.service.listar().subscribe({
      next: data => { this.encuestas.set(data); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar encuestas.'); this.cargando.set(false); }
    });
  }

  crearEncuesta(): void {
    if (!this.nuevaEncuesta.titulo.trim()) {
      this.error.set('El título es obligatorio.');
      return;
    }
    this.error.set(null);
    this.guardandoEncuesta.set(true);
    this.service.crear(this.nuevaEncuesta).subscribe({
      next: (e) => {
        this.encuestas.update(list => [...list, e]);
        this.mostrarFormEncuesta.set(false);
        this.resetFormEncuesta();
        this.guardandoEncuesta.set(false);
        this.exito.set(`Encuesta "${e.titulo}" creada.`);
        setTimeout(() => this.exito.set(null), 3000);
      },
      error: (e) => {
        this.error.set(e.error?.detail ?? 'Error al crear la encuesta.');
        this.guardandoEncuesta.set(false);
      }
    });
  }

  abrirFormPregunta(encuestaId: number): void {
    this.encuestaSeleccionadaId.set(encuestaId);
    this.nuevaPregunta = { encuestaId, texto: '', orden: this.proximoOrden(encuestaId), tipoPregunta: 'EscalaLikert', esObligatoria: true };
    this.mostrarFormPregunta.set(true);
    this.error.set(null);
  }

  agregarPregunta(): void {
    if (!this.nuevaPregunta.texto.trim()) {
      this.error.set('El texto de la pregunta es obligatorio.');
      return;
    }
    this.error.set(null);
    this.guardandoPregunta.set(true);
    this.service.agregarPregunta(this.nuevaPregunta).subscribe({
      next: (p) => {
        this.encuestas.update(list =>
          list.map(e => e.id === this.nuevaPregunta.encuestaId
            ? { ...e, preguntas: [...e.preguntas, p] }
            : e)
        );
        this.mostrarFormPregunta.set(false);
        this.guardandoPregunta.set(false);
        this.exito.set('Pregunta agregada.');
        setTimeout(() => this.exito.set(null), 3000);
      },
      error: (e) => {
        this.error.set(e.error?.detail ?? 'Error al agregar pregunta.');
        this.guardandoPregunta.set(false);
      }
    });
  }

  toggleActiva(encuesta: EncuestaDto): void {
    const accion = encuesta.activa
      ? this.service.desactivar(encuesta.id)
      : this.service.activar(encuesta.id);

    accion.subscribe({
      next: () => {
        this.encuestas.update(list =>
          list.map(e => e.id === encuesta.id ? { ...e, activa: !e.activa } : e)
        );
        this.exito.set(encuesta.activa ? 'Encuesta desactivada.' : 'Encuesta activada.');
        setTimeout(() => this.exito.set(null), 3000);
      },
      error: () => this.error.set('Error al cambiar estado.')
    });
  }

  private proximoOrden(encuestaId: number): number {
    const enc = this.encuestas().find(e => e.id === encuestaId);
    return enc ? enc.preguntas.length + 1 : 1;
  }

  private resetFormEncuesta(): void {
    this.nuevaEncuesta = {
      titulo: '', descripcion: null, tipo: 'SatisfaccionGeneral',
      cicloLectivo: new Date().getFullYear(), materiaId: null
    };
  }

  cancelarFormEncuesta(): void { this.mostrarFormEncuesta.set(false); this.resetFormEncuesta(); this.error.set(null); }
  cancelarFormPregunta(): void { this.mostrarFormPregunta.set(false); this.error.set(null); }

  tipoLabel(tipo: string): string {
    return tipo === 'EvaluacionDocente' ? 'Evaluación Docente' : 'Satisfacción General';
  }
}
