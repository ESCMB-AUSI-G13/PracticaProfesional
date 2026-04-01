import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export function roleGuard(...rolesPermitidos: string[]): CanActivateFn {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.estaAutenticado()) {
      return router.createUrlTree(['/login']);
    }

    // Usa el rol real para acceso a rutas (no el rolVista)
    // El modo vista solo afecta la UI, no los permisos de API
    const rolReal = authService.rol();
    if (rolReal && rolesPermitidos.includes(rolReal)) {
      return true;
    }

    return router.createUrlTree(['/dashboard']);
  };
}
