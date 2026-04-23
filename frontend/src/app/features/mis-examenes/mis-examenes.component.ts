import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MisExamenesService, ExamenFinalDisponible } from './mis-examenes.service';

@Component({
  selector: 'app-mis-examenes',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mis-examenes.component.html',
  styleUrl: './mis-examenes.component.scss'
})
export class MisExamenesComponent implements OnInit {
  examenes  = signal<ExamenFinalDisponible[]>([]);
  cargando  = signal(true);
  error     = signal<string | null>(null);
  inscribiendo = signal<number | null>(null);

  constructor(
    private service: MisExamenesService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.service.listarFinalesDisponibles().subscribe({
      next: data => { this.examenes.set(data); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar los exámenes disponibles.'); this.cargando.set(false); }
    });
  }

  inscribirse(examen: ExamenFinalDisponible): void {
    if (examen.yaInscripto || this.inscribiendo() !== null) return;
    this.inscribiendo.set(examen.id);
    this.error.set(null);
    this.service.inscribirse(examen.id).subscribe({
      next: () => {
        this.examenes.update(list =>
          list.map(e => e.id === examen.id ? { ...e, yaInscripto: true } : e)
        );
        this.inscribiendo.set(null);
      },
      error: (e) => {
        this.error.set(e.error?.mensaje ?? 'Error al inscribirse.');
        this.inscribiendo.set(null);
      }
    });
  }

  irAlDashboard(): void { this.router.navigate(['/dashboard']); }
}
