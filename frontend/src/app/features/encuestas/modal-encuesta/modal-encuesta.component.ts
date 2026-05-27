import { Component, EventEmitter, Input, OnInit, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EncuestasService, EncuestaDto, PreguntaEncuesta } from '../encuestas.service';

@Component({
  selector: 'app-modal-encuesta',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './modal-encuesta.component.html',
  styleUrl: './modal-encuesta.component.scss'
})
export class ModalEncuestaComponent implements OnInit {
  @Input()  encuesta!: EncuestaDto;
  @Output() completada = new EventEmitter<void>();
  @Output() cerrar     = new EventEmitter<void>();

  respuestas: Map<number, { valorNumerico: number | null; textoLibre: string }> = new Map();
  enviando  = signal(false);
  error     = signal<string | null>(null);

  ngOnInit(): void {
    this.encuesta.preguntas.forEach(p =>
      this.respuestas.set(p.id, { valorNumerico: null, textoLibre: '' })
    );
  }

  constructor(private encuestasService: EncuestasService) {}

  setLikert(preguntaId: number, valor: number): void {
    const r = this.respuestas.get(preguntaId)!;
    r.valorNumerico = r.valorNumerico === valor ? null : valor;
  }

  getLikert(preguntaId: number): number | null {
    return this.respuestas.get(preguntaId)?.valorNumerico ?? null;
  }

  getTexto(preguntaId: number): string {
    return this.respuestas.get(preguntaId)?.textoLibre ?? '';
  }

  setTexto(preguntaId: number, valor: string): void {
    this.respuestas.get(preguntaId)!.textoLibre = valor;
  }

  puedeEnviar(): boolean {
    return this.encuesta.preguntas
      .filter(p => p.esObligatoria)
      .every(p => {
        const r = this.respuestas.get(p.id);
        if (p.tipoPregunta === 'EscalaLikert') return r?.valorNumerico !== null;
        return (r?.textoLibre?.trim().length ?? 0) > 0;
      });
  }

  enviar(): void {
    if (!this.puedeEnviar()) return;

    this.enviando.set(true);
    this.error.set(null);

    const items = this.encuesta.preguntas.map(p => {
      const r = this.respuestas.get(p.id)!;
      return {
        preguntaId:    p.id,
        valorNumerico: p.tipoPregunta === 'EscalaLikert' ? r.valorNumerico : null,
        textoLibre:    p.tipoPregunta === 'TextoLibre'   ? r.textoLibre || null : null,
      };
    });

    this.encuestasService.responder({ encuestaId: this.encuesta.id, items }).subscribe({
      next:  () => { this.enviando.set(false); this.completada.emit(); },
      error: () => {
        this.enviando.set(false);
        this.error.set('No se pudo enviar la encuesta. Intentá nuevamente.');
      }
    });
  }

  likertLabels = ['', 'Muy malo', 'Malo', 'Regular', 'Bueno', 'Muy bueno'];
}
