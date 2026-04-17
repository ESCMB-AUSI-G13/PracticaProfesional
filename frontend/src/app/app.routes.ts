import { Routes } from '@angular/router';
import { authGuard } from './features/auth/guards/auth.guard';
import { roleGuard } from './features/auth/guards/role.guard';

export const routes: Routes = [
  // Rutas públicas
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'registro',
    loadComponent: () =>
      import('./features/auth/registro/registro.component').then(m => m.RegistroComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./features/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },

  // Rutas autenticadas
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },

  // Módulo de gestión de usuarios — solo Dirección
  {
    path: 'usuarios',
    canActivate: [roleGuard('Direccion')],
    loadComponent: () =>
      import('./features/usuarios/lista-usuarios/lista-usuarios.component').then(m => m.ListaUsuariosComponent)
  },
  {
    path: 'usuarios/nuevo',
    canActivate: [roleGuard('Direccion')],
    loadComponent: () =>
      import('./features/usuarios/crear-usuario/crear-usuario.component').then(m => m.CrearUsuarioComponent)
  },
  {
    path: 'usuarios/:id/editar',
    canActivate: [roleGuard('Direccion')],
    loadComponent: () =>
      import('./features/usuarios/editar-usuario/editar-usuario.component').then(m => m.EditarUsuarioComponent)
  },

  // Módulo de gestión de docentes — solo Dirección
  {
    path: 'docentes',
    canActivate: [roleGuard('Direccion')],
    loadComponent: () =>
      import('./features/docentes/lista-docentes/lista-docentes.component').then(m => m.ListaDocentesComponent)
  },
  {
    path: 'docentes/nuevo',
    canActivate: [roleGuard('Direccion')],
    loadComponent: () =>
      import('./features/docentes/crear-docente/crear-docente.component').then(m => m.CrearDocenteComponent)
  },
  {
    path: 'docentes/:id/editar',
    canActivate: [roleGuard('Direccion')],
    loadComponent: () =>
      import('./features/docentes/editar-docente/editar-docente.component').then(m => m.EditarDocenteComponent)
  },

  // Módulo de gestión de preceptores — solo Dirección
  {
    path: 'preceptores',
    canActivate: [roleGuard('Direccion')],
    loadComponent: () =>
      import('./features/preceptores/lista-preceptores/lista-preceptores.component').then(m => m.ListaPreceptoresComponent)
  },
  {
    path: 'preceptores/nuevo',
    canActivate: [roleGuard('Direccion')],
    loadComponent: () =>
      import('./features/preceptores/crear-preceptor/crear-preceptor.component').then(m => m.CrearPreceptorComponent)
  },
  {
    path: 'preceptores/:id/editar',
    canActivate: [roleGuard('Direccion')],
    loadComponent: () =>
      import('./features/preceptores/editar-preceptor/editar-preceptor.component').then(m => m.EditarPreceptorComponent)
  },

  // Módulo de gestión de estudiantes — solo Dirección
  {
    path: 'estudiantes',
    canActivate: [roleGuard('Direccion')],
    loadComponent: () =>
      import('./features/estudiantes/lista-estudiantes/lista-estudiantes.component').then(m => m.ListaEstudiantesComponent)
  },
  {
    path: 'estudiantes/nuevo',
    canActivate: [roleGuard('Direccion')],
    loadComponent: () =>
      import('./features/estudiantes/crear-estudiante/crear-estudiante.component').then(m => m.CrearEstudianteComponent)
  },
  {
    path: 'estudiantes/:id/editar',
    canActivate: [roleGuard('Direccion')],
    loadComponent: () =>
      import('./features/estudiantes/editar-estudiante/editar-estudiante.component').then(m => m.EditarEstudianteComponent)
  },

  // Logs de seguridad — solo Dirección
  {
    path: 'logs-seguridad',
    canActivate: [roleGuard('Direccion')],
    loadComponent: () =>
      import('./features/logs-seguridad/panel-logs-seguridad/panel-logs-seguridad.component')
        .then(m => m.PanelLogsSeguridadComponent)
  },

  // Auditoría — solo Dirección
  {
    path: 'auditoria',
    canActivate: [roleGuard('Direccion')],
    loadComponent: () =>
      import('./features/auditoria/panel-auditoria/panel-auditoria.component').then(m => m.PanelAuditoriaComponent)
  },

  // Módulo de calificaciones — solo Docente
  {
    path: 'calificaciones/carga-notas',
    canActivate: [roleGuard('Docente')],
    loadComponent: () =>
      import('./features/calificaciones/carga-notas/carga-notas.component')
        .then(m => m.CargaNotasComponent)
  },

  // Reportes operativos (RR-08, RR-09) — Preceptor y Dirección
  {
    path: 'reportes/inasistencias',
    canActivate: [roleGuard('Preceptor', 'Direccion')],
    loadComponent: () =>
      import('./features/reportes/panel-inasistencias/panel-inasistencias.component')
        .then(m => m.PanelInasistenciasComponent)
  },
  {
    path: 'reportes/control-legajo',
    canActivate: [roleGuard('Preceptor', 'Direccion')],
    loadComponent: () =>
      import('./features/reportes/control-legajo/control-legajo.component')
        .then(m => m.ControlLegajoComponent)
  },

  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: 'login' }
];
