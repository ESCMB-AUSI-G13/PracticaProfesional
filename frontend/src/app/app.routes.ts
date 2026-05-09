import { Routes } from '@angular/router';
import { authGuard } from './features/auth/guards/auth.guard';
import { roleGuard } from './features/auth/guards/role.guard';

export const routes: Routes = [
  // Rutas públicas (sin shell)
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

  // Rutas autenticadas — envueltas en el shell con sidebar
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./shared/shell/shell.component').then(m => m.ShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },

      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },

      // Usuarios — solo Dirección
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

      // Docentes — solo Dirección
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

      // Preceptores — solo Dirección
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

      // Estudiantes — solo Dirección
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

      // Calendario Académico — Dirección gestiona, otros roles solo visualizan
      {
        path: 'calendario',
        canActivate: [roleGuard('Direccion', 'Docente', 'Preceptor', 'Estudiante')],
        loadComponent: () =>
          import('./features/calendario/lista-calendario/lista-calendario.component').then(m => m.ListaCalendarioComponent)
      },
      {
        path: 'calendario/nuevo',
        canActivate: [roleGuard('Direccion')],
        loadComponent: () =>
          import('./features/calendario/crear-evento/crear-evento.component').then(m => m.CrearEventoComponent)
      },
      {
        path: 'calendario/:id/editar',
        canActivate: [roleGuard('Direccion')],
        loadComponent: () =>
          import('./features/calendario/crear-evento/crear-evento.component').then(m => m.CrearEventoComponent)
      },

      // Auditoría — solo Dirección
      {
        path: 'auditoria',
        canActivate: [roleGuard('Direccion')],
        loadComponent: () =>
          import('./features/auditoria/panel-auditoria/panel-auditoria.component').then(m => m.PanelAuditoriaComponent)
      },

      // Calificaciones — solo Docente
      {
        path: 'calificaciones/carga-notas',
        canActivate: [roleGuard('Docente')],
        loadComponent: () =>
          import('./features/calificaciones/carga-notas/carga-notas.component')
            .then(m => m.CargaNotasComponent)
      },

      // Materias — solo Dirección
      {
        path: 'materias',
        canActivate: [roleGuard('Direccion')],
        loadComponent: () =>
          import('./features/materias/lista-materias/lista-materias.component').then(m => m.ListaMateriasComponent)
      },
      {
        path: 'materias/nueva',
        canActivate: [roleGuard('Direccion')],
        loadComponent: () =>
          import('./features/materias/crear-materia/crear-materia.component').then(m => m.CrearMateriaComponent)
      },
      {
        path: 'materias/:id/editar',
        canActivate: [roleGuard('Direccion')],
        loadComponent: () =>
          import('./features/materias/editar-materia/editar-materia.component').then(m => m.EditarMateriaComponent)
      },

      // Cursos — solo Dirección
      {
        path: 'cursos',
        canActivate: [roleGuard('Direccion')],
        loadComponent: () =>
          import('./features/cursos/lista-cursos/lista-cursos.component').then(m => m.ListaCursosComponent)
      },
      {
        path: 'cursos/nuevo',
        canActivate: [roleGuard('Direccion')],
        loadComponent: () =>
          import('./features/cursos/crear-curso/crear-curso.component').then(m => m.CrearCursoComponent)
      },
      {
        path: 'cursos/:id/editar',
        canActivate: [roleGuard('Direccion')],
        loadComponent: () =>
          import('./features/cursos/editar-curso/editar-curso.component').then(m => m.EditarCursoComponent)
      },

      // Inscripciones a Materias — solo Dirección
      {
        path: 'inscripciones-materia',
        canActivate: [roleGuard('Direccion')],
        loadComponent: () =>
          import('./features/inscripciones-materia/lista-inscripciones/lista-inscripciones.component').then(m => m.ListaInscripcionesComponent)
      },

      // Espacios Curriculares — solo Dirección
      {
        path: 'espacios-curriculares',
        canActivate: [roleGuard('Direccion')],
        loadComponent: () =>
          import('./features/espacios-curriculares/lista-espacios/lista-espacios.component').then(m => m.ListaEspaciosComponent)
      },

      // Exámenes — Dirección y Docente
      {
        path: 'examenes',
        canActivate: [roleGuard('Direccion', 'Docente')],
        loadComponent: () =>
          import('./features/examenes/lista-examenes/lista-examenes.component').then(m => m.ListaExamenesComponent)
      },

      // Mis Materias — solo Estudiante
      {
        path: 'mis-materias',
        canActivate: [roleGuard('Estudiante')],
        loadComponent: () =>
          import('./features/mis-materias/mis-materias.component').then(m => m.MisMateriasComponent)
      },

      // Mis Exámenes — solo Estudiante
      {
        path: 'mis-examenes',
        canActivate: [roleGuard('Estudiante')],
        loadComponent: () =>
          import('./features/mis-examenes/mis-examenes.component').then(m => m.MisExamenesComponent)
      },

      // Reportes — Preceptor y Dirección
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

      // Reportes — Dirección y Docente
      {
        path: 'reportes/comisiones',
        canActivate: [roleGuard('Direccion', 'Docente')],
        loadComponent: () =>
          import('./features/reportes/panel-comisiones/panel-comisiones.component')
            .then(m => m.PanelComisionesComponent)
      },
      {
        path: 'reportes/evolucion',
        canActivate: [roleGuard('Direccion', 'Docente')],
        loadComponent: () =>
          import('./features/reportes/panel-evolucion/panel-evolucion.component')
            .then(m => m.PanelEvolucionComponent)
      },
      {
        path: 'reportes/catedras',
        canActivate: [roleGuard('Direccion', 'Docente')],
        loadComponent: () =>
          import('./features/reportes/panel-catedras/panel-catedras.component')
            .then(m => m.PanelCatedrasComponent)
      },
    ]
  },

  { path: '**', redirectTo: 'login' }
];
