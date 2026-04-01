import { Component, inject } from '@angular/core';
import { AuthService } from '../../features/auth/services/auth.service';

@Component({
  selector: 'app-banner-vista-rol',
  standalone: true,
  templateUrl: './banner-vista-rol.component.html',
  styleUrl: './banner-vista-rol.component.scss'
})
export class BannerVistaRolComponent {
  authService = inject(AuthService);
}
