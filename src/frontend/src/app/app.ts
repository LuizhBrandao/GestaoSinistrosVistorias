import { Component } from '@angular/core';
import { NavbarComponent } from './components/navbar/navbar.component';
import { LaudoVistoriaComponent } from './components/laudo-vistoria/laudo-vistoria.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NavbarComponent, LaudoVistoriaComponent],
  template: `
    <app-navbar></app-navbar>
    <main>
      <app-laudo-vistoria></app-laudo-vistoria>
    </main>
  `
})
export class App {}
