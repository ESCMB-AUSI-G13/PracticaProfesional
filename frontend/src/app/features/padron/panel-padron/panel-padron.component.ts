import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PadronService, PadronAlumnoDto, ImportarPadronResultDto } from '../padron.service';

@Component({
  selector: 'app-panel-padron',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './panel-padron.component.html',
  styleUrl: './panel-padron.component.scss'
})
export class PanelPadronComponent {
  private readonly padronService = inject(PadronService);

  padron = signal<PadronAlumnoDto[]>([]);
  cargando = signal(false);
  error = signal<string | null>(null);

  // Alta manual
  dniManual = signal('');
  guardandoManual = signal(false);
  mensajeManual = signal<string | null>(null);
  errorManual = signal<string | null>(null);

  // Importar CSV
  archivoSeleccionado = signal<File | null>(null);
  importando = signal(false);
  resultadoImport = signal<ImportarPadronResultDto | null>(null);

  // Búsqueda
  busqueda = signal('');
  padronFiltrado = computed(() => {
    const q = this.busqueda().toLowerCase();
    return this.padron().filter(p => p.dni.includes(q));
  });

  ngOnInit(): void {
    this.cargarPadron();
  }

  cargarPadron(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.padronService.listar().subscribe({
      next: data => {
        this.padron.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar el padrón.');
        this.cargando.set(false);
      }
    });
  }

  agregarManual(): void {
    const dni = this.dniManual().trim();
    if (!dni) return;
    this.guardandoManual.set(true);
    this.mensajeManual.set(null);
    this.errorManual.set(null);

    this.padronService.agregarDni(dni).subscribe({
      next: () => {
        this.mensajeManual.set(`DNI ${dni} agregado correctamente.`);
        this.dniManual.set('');
        this.guardandoManual.set(false);
        this.cargarPadron();
      },
      error: err => {
        this.errorManual.set(err.error?.detail ?? 'No se pudo agregar el DNI.');
        this.guardandoManual.set(false);
      }
    });
  }

  onArchivoSeleccionado(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.archivoSeleccionado.set(input.files?.[0] ?? null);
    this.resultadoImport.set(null);
  }

  importar(): void {
    const archivo = this.archivoSeleccionado();
    if (!archivo) return;
    this.importando.set(true);
    this.resultadoImport.set(null);

    this.padronService.importarCsv(archivo).subscribe({
      next: resultado => {
        this.resultadoImport.set(resultado);
        this.importando.set(false);
        this.archivoSeleccionado.set(null);
        this.cargarPadron();
      },
      error: err => {
        this.resultadoImport.set(null);
        this.error.set(err.error?.detail ?? 'Error al importar el archivo.');
        this.importando.set(false);
      }
    });
  }

  eliminar(dni: string): void {
    if (!confirm(`¿Eliminar el DNI ${dni} del padrón?`)) return;
    this.padronService.eliminar(dni).subscribe({
      next: () => this.cargarPadron(),
      error: err => this.error.set(err.error?.detail ?? 'No se pudo eliminar el DNI.')
    });
  }
}
