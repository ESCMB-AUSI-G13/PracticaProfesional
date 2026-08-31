import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-cargando',
  standalone: true,
  templateUrl: './cargando.component.html',
  styleUrl: './cargando.component.scss'
})
export class CargandoComponent {
  @Input() mensaje = 'Cargando...';
}
