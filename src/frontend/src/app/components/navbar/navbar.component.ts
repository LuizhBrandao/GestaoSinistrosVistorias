import { Component, inject } from '@angular/core';
import { WebSocketService } from '../../services/websocket.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark shadow-sm border-bottom">
      <div class="container">
        <a class="navbar-brand d-flex align-items-center gap-2 fw-bold" href="#">
          <i class="bi bi-camera-reels text-warning fs-4"></i>
          <span>Vistoria<span class="text-warning">Express</span></span>
          <span class="badge bg-secondary small ms-2">Angular Signals</span>
        </a>

        <div class="d-flex align-items-center gap-3 ms-auto">
          <!-- WebSocket Status Badge -->
          @if (wsService.isConnected()) {
            <span class="badge bg-success-subtle text-success border border-success-subtle d-flex align-items-center gap-1">
              <span class="spinner-grow spinner-grow-sm text-success" style="width: 0.5rem; height: 0.5rem;"></span>
              WebSocket Ativo (BFF Fastify)
            </span>
          } @else {
            <span class="badge bg-danger-subtle text-danger border border-danger-subtle d-flex align-items-center gap-1">
              <i class="bi bi-x-circle-fill"></i>
              WebSocket Offline
            </span>
          }

          <a href="http://localhost:5000" target="_blank" class="btn btn-sm btn-outline-light d-flex align-items-center gap-1">
            <i class="bi bi-box-arrow-up-right"></i>
            Back-Office .NET 10
          </a>
        </div>
      </div>
    </nav>
  `
})
export class NavbarComponent {
  public readonly wsService = inject(WebSocketService);
}
