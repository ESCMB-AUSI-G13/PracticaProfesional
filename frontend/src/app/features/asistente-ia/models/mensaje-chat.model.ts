export interface MensajeChat {
  rol: 'usuario' | 'asistente' | 'error';
  texto: string;
  timestamp: Date;
}
