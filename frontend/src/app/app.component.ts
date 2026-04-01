import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BannerVistaRolComponent } from './shared/banner-vista-rol/banner-vista-rol.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, BannerVistaRolComponent],
  template: `
    <app-banner-vista-rol />
    <router-outlet />
  `
})
export class AppComponent {}
