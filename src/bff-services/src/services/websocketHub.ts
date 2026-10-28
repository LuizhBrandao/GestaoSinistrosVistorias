import { WebSocket } from 'ws';

export interface WebSocketMessage {
  type: string;
  payload: unknown;
  timestamp: string;
}

export class WebSocketHub {
  private clients: Set<WebSocket> = new Set();

  register(socket: WebSocket): void {
    this.clients.add(socket);

    // Envia mensagem de boas-vindas com status de conexão
    this.send(socket, {
      type: 'CONEXAO_ESTABELECIDA',
      payload: { mensagem: 'Conectado ao gateway de tempo real de vistorias.' },
      timestamp: new Date().toISOString()
    });

    socket.on('close', () => {
      this.clients.delete(socket);
    });

    socket.on('error', () => {
      this.clients.delete(socket);
    });
  }

  broadcast(type: string, payload: unknown): void {
    const message: WebSocketMessage = {
      type,
      payload,
      timestamp: new Date().toISOString()
    };

    const data = JSON.stringify(message);

    for (const client of this.clients) {
      if (client.readyState === WebSocket.OPEN) {
        client.send(data);
      }
    }
  }

  private send(socket: WebSocket, message: WebSocketMessage): void {
    if (socket.readyState === WebSocket.OPEN) {
      socket.send(JSON.stringify(message));
    }
  }

  get connectedCount(): number {
    return this.clients.size;
  }
}

export const wsHub = new WebSocketHub();
