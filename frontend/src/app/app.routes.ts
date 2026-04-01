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

  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: 'login' }
];
