import { Component, inject, computed, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { AuthService } from '../../features/auth/services/auth.service';
import { BannerVistaRolComponent } from '../banner-vista-rol/banner-vista-rol.component';
import { NotificacionBellComponent } from '../notificacion-bell/notificacion-bell.component';
import { NotificacionesService } from '../../features/notificaciones/notificaciones.service';

interface NavItem {
  label: string;
  ruta?: string;
  icon: string;
  roles: string[];
  children?: NavItem[];
}

const ICONS: Record<string, string> = {
  inicio: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>`,
  entidades: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="3" width="20" height="14" rx="2" ry="2"/><line x1="8" y1="21" x2="16" y2="21"/><line x1="12" y1="17" x2="12" y2="21"/></svg>`,
  calificaciones: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>`,
  examenes: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="9" y1="13" x2="15" y2="13"/><line x1="9" y1="17" x2="12" y2="17"/></svg>`,
  reportes: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/><line x1="6" y1="20" x2="6" y2="14"/></svg>`,
  auditoria: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>`,
  logs: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>`,
  calendario: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>`,
  asistencias: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><polyline points="16 11 18 13 22 9"/></svg>`,
};

const NAV: NavItem[] = [
  {
    label: 'Inicio',
    ruta: '/dashboard',
    icon: 'inicio',
    roles: ['Direccion', 'Docente', 'Estudiante', 'Preceptor']
  },
  {
    label: 'Gestión de Entidades',
    icon: 'entidades',
    roles: ['Direccion'],
    children: [
      { label: 'Usuarios',      ruta: '/usuarios',              icon: '', roles: ['Direccion'] },
      { label: 'Docentes',      ruta: '/docentes',              icon: '', roles: ['Direccion'] },
      { label: 'Preceptores',   ruta: '/preceptores',           icon: '', roles: ['Direccion'] },
      { label: 'Estudiantes',   ruta: '/estudiantes',           icon: '', roles: ['Direccion'] },
      { label: 'Materias',      ruta: '/materias',              icon: '', roles: ['Direccion'] },
      { label: 'Cursos',        ruta: '/cursos',                icon: '', roles: ['Direccion'] },
      { label: 'Cátedras',      ruta: '/espacios-curriculares', icon: '', roles: ['Direccion'] },
      { label: 'Inscripciones', ruta: '/inscripciones-materia', icon: '', roles: ['Direccion'] },
    ]
  },
  {
    label: 'Calificaciones',
    ruta: '/calificaciones/carga-notas',
    icon: 'calificaciones',
    roles: ['Docente']
  },
  {
    label: 'Asistencias',
    icon: 'asistencias',
    roles: ['Docente', 'Preceptor'],
    children: [
      { label: 'Registrar',  ruta: '/asistencias/registrar',  icon: '', roles: ['Docente'] },
      { label: 'Rectificar', ruta: '/asistencias/rectificar', icon: '', roles: ['Docente', 'Preceptor'] },
    ]
  },
  {
    label: 'Exámenes',
    ruta: '/examenes',
    icon: 'examenes',
    roles: ['Direccion', 'Docente']
  },
  {
    label: 'Mis Materias',
    ruta: '/mis-materias',
    icon: 'calificaciones',
    roles: ['Estudiante']
  },
  {
    label: 'Mis Exámenes',
    ruta: '/mis-examenes',
    icon: 'examenes',
    roles: ['Estudiante']
  },
  {
    label: 'Reportes',
    icon: 'reportes',
    roles: ['Direccion', 'Docente', 'Preceptor'],
    children: [
      { label: 'Inasistencias',      ruta: '/reportes/inasistencias',  icon: '', roles: ['Direccion', 'Preceptor', 'Docente'] },
      { label: 'Control por Legajo', ruta: '/reportes/control-legajo', icon: '', roles: ['Direccion', 'Preceptor'] },
      { label: 'Comisiones',         ruta: '/reportes/comisiones',     icon: '', roles: ['Direccion', 'Docente'] },
      { label: 'Evolución de Notas', ruta: '/reportes/evolucion',      icon: '', roles: ['Direccion', 'Docente'] },
      { label: 'Promedios Cátedra',  ruta: '/reportes/catedras',       icon: '', roles: ['Direccion', 'Docente'] },
    ]
  },
  {
    label: 'Calendario Académico',
    ruta: '/calendario',
    icon: 'calendario',
    roles: ['Direccion', 'Docente', 'Preceptor', 'Estudiante']
  },
  {
    label: 'Auditoría',
    ruta: '/auditoria',
    icon: 'auditoria',
    roles: ['Direccion']
  },
  {
    label: 'Logs de Seguridad',
    ruta: '/logs-seguridad',
    icon: 'logs',
    roles: ['Direccion']
  },
];

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, BannerVistaRolComponent, NotificacionBellComponent],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss'
})
export class ShellComponent implements OnInit, OnDestroy {
  authService = inject(AuthService);
  private sanitizer = inject(DomSanitizer);
  private notifSvc = inject(NotificacionesService);

  ngOnInit(): void { this.notifSvc.iniciarPolling(); }
  ngOnDestroy(): void { this.notifSvc.detenerPolling(); }

  sidebarOpen = false;
  expandedGroups: string[] = ['Gestión de Entidades'];

  itemsVisibles = computed(() => {
    const rol = this.authService.rolVista();
    if (!rol) return [];
    return NAV
      .filter(item => item.roles.includes(rol))
      .map(item => ({
        ...item,
        children: item.children?.filter(c => c.roles.includes(rol))
      }));
  });

  get iniciales(): string {
    const nombre = this.authService.usuario()?.nombreCompleto ?? '';
    return nombre.split(' ').map(p => p[0]).filter(Boolean).slice(0, 2).join('').toUpperCase() || '?';
  }

  getIcon(name: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(ICONS[name] ?? '');
  }

  isExpanded(label: string): boolean {
    return this.expandedGroups.includes(label);
  }

  toggleGroup(label: string): void {
    const idx = this.expandedGroups.indexOf(label);
    if (idx >= 0) this.expandedGroups.splice(idx, 1);
    else this.expandedGroups.push(label);
  }

  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
  }

  closeSidebar(): void {
    this.sidebarOpen = false;
  }

  activarVista(event: Event): void {
    const rol = (event.target as HTMLSelectElement).value;
    if (rol) this.authService.activarVista(rol);
  }
}
