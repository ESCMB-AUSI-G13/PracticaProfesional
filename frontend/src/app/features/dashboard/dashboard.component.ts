import { Component, inject, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../auth/services/auth.service';

interface Modulo {
  titulo: string;
  descripcion: string;
  ruta: string;
  roles: string[];
}

const MODULOS: Modulo[] = [
  {
    titulo: 'Gestión de Usuarios',
    descripcion: 'Alta, baja y modificación de todos los usuarios del sistema.',
    ruta: '/usuarios',
    roles: ['Direccion']
  },
  {
    titulo: 'Gestión de Docentes',
    descripcion: 'Alta, baja y modificación de docentes. Categoría y datos de contacto.',
    ruta: '/docentes',
    roles: ['Direccion']
  },
  {
    titulo: 'Gestión de Preceptores',
    descripcion: 'Alta, baja y modificación de preceptores. Turno y datos de contacto.',
    ruta: '/preceptores',
    roles: ['Direccion']
  },
  {
    titulo: 'Gestión de Estudiantes',
    descripcion: 'Alta, baja y modificación de estudiantes. Año, condición y fecha de ingreso.',
    ruta: '/estudiantes',
    roles: ['Direccion']
  },
  {
    titulo: 'Auditoría de Cambios',
    descripcion: 'Historial completo de modificaciones sobre datos críticos. Trazabilidad por entidad, acción y ejecutor.',
    ruta: '/auditoria',
    roles: ['Direccion']
  },
  {
    titulo: 'Logs de Seguridad',
    descripcion: 'Registro de intentos de inicio de sesión exitosos y fallidos. Detecta accesos no autorizados con IP y User-Agent.',
    ruta: '/logs-seguridad',
    roles: ['Direccion']
  },
  {
    titulo: 'Carga de Calificaciones',
    descripcion: 'Registrá las notas de parciales, recuperatorios y exámenes finales de tus alumnos.',
    ruta: '/calificaciones/carga-notas',
    roles: ['Docente']
  },
  {
    titulo: 'Reporte de Inasistencias',
    descripcion: 'Listado detallado de ausencias con filtros por curso, materia y rango de fechas. Incluye justificaciones.',
    ruta: '/reportes/inasistencias',
    roles: ['Preceptor', 'Direccion']
  },
  {
    titulo: 'Control por Legajo',
    descripcion: 'Ficha individual de asistencia por legajo: porcentajes, alertas de riesgo y estado de regularidad por materia.',
    ruta: '/reportes/control-legajo',
    roles: ['Preceptor', 'Direccion']
  },
  {
    titulo: 'Comparativo de Comisiones',
    descripcion: 'Compará el rendimiento entre comisiones: inscriptos, aprobados, desaprobados y promedio general. (RR-05)',
    ruta: '/reportes/comisiones',
    roles: ['Direccion', 'Docente']
  },
  {
    titulo: 'Evolución de Notas',
    descripcion: 'Seguí cómo evolucionaron los promedios y la tasa de aprobación período a período. (RR-06)',
    ruta: '/reportes/evolucion',
    roles: ['Direccion', 'Docente']
  },
  {
    titulo: 'Promedios por Cátedra',
    descripcion: 'Promedio y % de aprobación por materia, docente y comisión. Detalle completo por cátedra. (RR-07)',
    ruta: '/reportes/catedras',
    roles: ['Direccion', 'Docente']
  }
];

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {
  authService = inject(AuthService);

  // Filtra módulos por rolVista (no por rol real)
  modulosVisibles = computed(() => {
    const rol = this.authService.rolVista();
    if (!rol) return [];
    return MODULOS.filter(m => m.roles.includes(rol));
  });

  activarVista(event: Event): void {
    const rol = (event.target as HTMLSelectElement).value;
    if (rol) this.authService.activarVista(rol);
  }
}
