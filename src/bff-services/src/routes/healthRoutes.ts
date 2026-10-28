import { FastifyInstance, FastifyPluginAsync } from 'fastify';
import { wsHub } from '../services/websocketHub.js';

export const healthRoutes: FastifyPluginAsync = async (server: FastifyInstance) => {
  server.get('/health', async () => {
    return {
      status: 'OK',
      servico: 'Sinistros BFF & Image Processing Worker',
      uptimeSegundos: Math.floor(process.uptime()),
      conexoesWebSocketAtivas: wsHub.connectedCount,
      memoriaMB: Math.round(process.memoryUsage().rss / 1024 / 1024),
      versaoNode: process.version
    };
  });
};
