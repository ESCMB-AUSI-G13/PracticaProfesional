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

    const rolUsuario = authService.rol();
    if (rolUsuario && rolesPermitidos.includes(rolUsuario)) {
      return true;
    }

    return router.createUrlTree(['/dashboard']);
  };
}
