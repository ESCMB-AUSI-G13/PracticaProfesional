import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MateriasService, Materia } from '../materias.service';

@Component({
  selector: 'app-lista-materias',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './lista-materias.component.html',
  styleUrl: './lista-materias.component.scss'
})
export class ListaMateriasComponent implements OnInit {
  materias = signal<Materia[]>([]);
  cargando = signal(true);
  error    = signal<string | null>(null);

  constructor(private materiasService: MateriasService, private router: Router) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.materiasService.listar().subscribe({
      next: data => { this.materias.set(data); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar las materias.'); this.cargando.set(false); }
    });
  }

  irACrear(): void    { this.router.navigate(['/materias/nueva']); }
  irAEditar(id: number): void { this.router.navigate(['/materias', id, 'editar']); }
  irAlDashboard(): void { this.router.navigate(['/dashboard']); }
}
