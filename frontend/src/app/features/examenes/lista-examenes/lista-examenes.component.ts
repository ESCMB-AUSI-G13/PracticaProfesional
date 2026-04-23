import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ExamenesService, Examen, CrearExamenRequest, TIPOS_EXAMEN } from '../examenes.service';
import { MateriasService, Materia } from '../../materias/materias.service';

@Component({
  selector: 'app-lista-examenes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './lista-examenes.component.html',
  styleUrl: './lista-examenes.component.scss'
})
export class ListaExamenesComponent implements OnInit {
  examenes   = signal<Examen[]>([]);
  materias   = signal<Materia[]>([]);
  tiposExamen = TIPOS_EXAMEN;

  cargando    = signal(true);
  mostrarForm = signal(false);
  guardando   = signal(false);
  error       = signal<string | null>(null);

  nuevoMateriaId  = signal<number | null>(null);
  nuevoFecha      = signal('');
  nuevoHorario    = signal('');
  nuevoCupo       = signal<number>(30);
  nuevoTipo       = signal('ParcialEscrito');

  constructor(
    private service: ExamenesService,
    private materiasService: MateriasService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.materiasService.listar().subscribe({
      next: m => this.materias.set(m),
      error: () => {}
    });
    this.service.listar().subscribe({
      next: e => { this.examenes.set(e); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar exámenes.'); this.cargando.set(false); }
    });
  }

  guardar(): void {
    if (!this.nuevoMateriaId() || !this.nuevoFecha() || !this.nuevoHorario() || !this.nuevoTipo()) {
      this.error.set('Completá todos los campos requeridos.');
      return;
    }
    this.guardando.set(true);
    this.error.set(null);
    const dto: CrearExamenRequest = {
      materiaId:   this.nuevoMateriaId()!,
      fechaExamen: this.nuevoFecha(),
      horario:     this.nuevoHorario(),
      cupo:        this.nuevoCupo(),
      tipoExamen:  this.nuevoTipo()
    };
    this.service.crear(dto).subscribe({
      next: ex => {
        this.examenes.update(list => [ex, ...list]);
        this.limpiarForm();
        this.guardando.set(false);
      },
      error: (e) => { this.error.set(e.error?.mensaje ?? 'Error al crear el examen.'); this.guardando.set(false); }
    });
  }

  limpiarForm(): void {
    this.nuevoMateriaId.set(null);
    this.nuevoFecha.set('');
    this.nuevoHorario.set('');
    this.nuevoCupo.set(30);
    this.nuevoTipo.set('ParcialEscrito');
    this.mostrarForm.set(false);
  }

  irAlDashboard(): void { this.router.navigate(['/dashboard']); }
}
