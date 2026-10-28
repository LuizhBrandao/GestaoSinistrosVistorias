import { Injectable, signal } from '@angular/core';
import { Observable, Subject } from 'rxjs';

export interface WebSocketEvent<T = any> {
  type: string;
  payload: T;
  timestamp: string;
}

@Injectable({
  providedIn: 'root'
})
export class WebSocketService {
  private socket: WebSocket | null = null;
  private messagesSubject = new Subject<WebSocketEvent>();
  
  // Angular Signal para reatividade instantânea no layout
  public readonly isConnected = signal<boolean>(false);
  public readonly lastEvent = signal<string | null>(null);

  constructor() {
    this.connect();
  }

  public get messages$(): Observable<WebSocketEvent> {
    return this.messagesSubject.asObservable();
  }

  public connect(): void {
    if (this.socket && (this.socket.readyState === WebSocket.OPEN || this.socket.readyState === WebSocket.CONNECTING)) {
      return;
    }

    try {
      this.socket = new WebSocket('ws://localhost:3000/ws');

      this.socket.onopen = () => {
        this.isConnected.set(true);
      };

      this.socket.onmessage = (event) => {
        try {
          const parsed: WebSocketEvent = JSON.parse(event.data);
          this.lastEvent.set(parsed.type);
          this.messagesSubject.next(parsed);
        } catch (err) {
          console.error('[WebSocket] Erro ao parsear mensagem:', err);
        }
      };

      this.socket.onclose = () => {
        this.isConnected.set(false);
        // Reconexão automática resiliente após 3 segundos
        setTimeout(() => this.connect(), 3000);
      };

      this.socket.onerror = (err) => {
        console.warn('[WebSocket] Conexão com BFF indisponível no momento.', err);
        this.socket?.close();
      };
    } catch (e) {
      this.isConnected.set(false);
      setTimeout(() => this.connect(), 3000);
    }
  }
}
