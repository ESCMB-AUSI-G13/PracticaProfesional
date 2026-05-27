import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  EncuestasService, EncuestaDto, MateriaEncuestaDto,
  CrearEncuestaRequest, AgregarPreguntaRequest
} from '../encuestas.service';

@Component({
  selector: 'app-panel-encuestas-docente',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-encuestas-docente.component.html',
  styleUrl:    './panel-encuestas-docente.component.scss'
})
export class PanelEncuestasDocenteComponent implements OnInit {
  encuestas = signal<EncuestaDto[]>([]);
  materias  = signal<MateriaEncuestaDto[]>([]);
  cargando  = signal(true);
  error     = signal<string | null>(null);
  exito     = signal<string | null>(null);

  mostrarFormEncuesta = signal(false);
  guardandoEncuesta   = signal(false);
  nuevaEncuesta: CrearEncuestaRequest = {
    titulo: '', descripcion: null, tipo: 'EvaluacionDocente',
    cicloLectivo: new Date().getFullYear(), materiaId: null
  };

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
    this.service.listarMisEncuestasDocente().subscribe({
      next: data => { this.encuestas.set(data); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar encuestas.'); this.cargando.set(false); }
    });
    this.service.listarMisMateriasDocente().subscribe({
      next: data => this.materias.set(data),
      error: () => {}
    });
  }

  abrirFormEncuesta(): void {
    this.nuevaEncuesta = {
      titulo: '', descripcion: null, tipo: 'EvaluacionDocente',
      cicloLectivo: new Date().getFullYear(), materiaId: null
    };
    this.error.set(null);
    this.mostrarFormEncuesta.set(true);
  }

  crearEncuesta(): void {
    if (!this.nuevaEncuesta.titulo.trim()) { this.error.set('El título es obligatorio.'); return; }
    if (!this.nuevaEncuesta.materiaId)     { this.error.set('Seleccioná una materia.'); return; }
    this.error.set(null);
    this.guardandoEncuesta.set(true);
    this.service.crearDocente(this.nuevaEncuesta).subscribe({
      next: e => {
        this.encuestas.update(list => [e, ...list]);
        this.mostrarFormEncuesta.set(false);
        this.guardandoEncuesta.set(false);
        this.exito.set(`Encuesta "${e.titulo}" creada.`);
        setTimeout(() => this.exito.set(null), 3000);
      },
      error: e => {
        this.error.set(e.error?.detail ?? 'Error al crear la encuesta.');
        this.guardandoEncuesta.set(false);
      }
    });
  }

  abrirFormPregunta(encuestaId: number): void {
    this.encuestaSeleccionadaId.set(encuestaId);
    this.nuevaPregunta = {
      encuestaId, texto: '', orden: this.proximoOrden(encuestaId),
      tipoPregunta: 'EscalaLikert', esObligatoria: true
    };
    this.mostrarFormPregunta.set(true);
    this.error.set(null);
  }

  agregarPregunta(): void {
    if (!this.nuevaPregunta.texto.trim()) { this.error.set('El texto es obligatorio.'); return; }
    this.error.set(null);
    this.guardandoPregunta.set(true);
    this.service.agregarPreguntaDocente(this.nuevaPregunta).subscribe({
      next: p => {
        this.encuestas.update(list =>
          list.map(e => e.id === this.nuevaPregunta.encuestaId
            ? { ...e, preguntas: [...e.preguntas, p] } : e)
        );
        this.mostrarFormPregunta.set(false);
        this.guardandoPregunta.set(false);
        this.exito.set('Pregunta agregada.');
        setTimeout(() => this.exito.set(null), 3000);
      },
      error: e => {
        this.error.set(e.error?.detail ?? 'Error al agregar pregunta.');
        this.guardandoPregunta.set(false);
      }
    });
  }

  toggleActiva(encuesta: EncuestaDto): void {
    const accion = encuesta.activa
      ? this.service.desactivarDocente(encuesta.id)
      : this.service.activarDocente(encuesta.id);
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

  cancelarFormEncuesta(): void { this.mostrarFormEncuesta.set(false); this.error.set(null); }
  cancelarFormPregunta(): void { this.mostrarFormPregunta.set(false); this.error.set(null); }

  private proximoOrden(encuestaId: number): number {
    const enc = this.encuestas().find(e => e.id === encuestaId);
    return enc ? enc.preguntas.length + 1 : 1;
  }
}
