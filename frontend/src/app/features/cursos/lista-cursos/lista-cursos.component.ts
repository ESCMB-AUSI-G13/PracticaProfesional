import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CursosService, Curso } from '../cursos.service';

@Component({
  selector: 'app-lista-cursos',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './lista-cursos.component.html',
  styleUrl: './lista-cursos.component.scss'
})
export class ListaCursosComponent implements OnInit {
  cursos   = signal<Curso[]>([]);
  cargando = signal(true);
  error    = signal<string | null>(null);

  constructor(private cursosService: CursosService, private router: Router) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.cursosService.listar().subscribe({
      next: data => { this.cursos.set(data); this.cargando.set(false); },
      error: () => { this.error.set('Error al cargar los cursos.'); this.cargando.set(false); }
    });
  }

  cerrar(id: number): void {
    if (!confirm('¿Cerrar este curso?')) return;
    this.cursosService.cerrar(id).subscribe({ next: () => this.cargar(), error: () => this.error.set('Error al cerrar.') });
  }

  reactivar(id: number): void {
    if (!confirm('¿Reactivar este curso?')) return;
    this.cursosService.reactivar(id).subscribe({ next: () => this.cargar(), error: () => this.error.set('Error al reactivar.') });
  }

  irACrear(): void          { this.router.navigate(['/cursos/nuevo']); }
  irAEditar(id: number): void { this.router.navigate(['/cursos', id, 'editar']); }
  irAlDashboard(): void     { this.router.navigate(['/dashboard']); }
}
