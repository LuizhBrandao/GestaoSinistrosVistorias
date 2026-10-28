import fastifyCors from '@fastify/cors';
import fastifyMultipart from '@fastify/multipart';
import fastifyStatic from '@fastify/static';
import fastifyWebsocket from '@fastify/websocket';
import Fastify, { FastifyInstance } from 'fastify';
import path from 'path';
import { env } from './config/env.js';
import { healthRoutes } from './routes/healthRoutes.js';
import { uploadRoutes } from './routes/uploadRoutes.js';
import { vistoriaRoutes } from './routes/vistoriaRoutes.js';
import { imageProcessor } from './services/imageProcessor.js';
import { rabbitMq } from './services/rabbitmqService.js';
import { wsHub } from './services/websocketHub.js';

export async function createServer(): Promise<FastifyInstance> {
  const server = Fastify({
    logger: {
      level: 'info',
      transport: {
        target: 'pino-pretty',
        options: {
          colorize: true,
          translateTime: 'HH:MM:ss Z',
          ignore: 'pid,hostname'
        }
      }
    }
  });

  // 1. Inicializa diretórios de armazenamento e conexão de mensageria
  await imageProcessor.init();
  await rabbitMq.init();

  // 2. Registra CORS para permitir requisições do Angular SPA
  await server.register(fastifyCors, {
    origin: true,
    methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS']
  });

  // 3. Suporte a Multipart Upload de alta performance
  await server.register(fastifyMultipart, {
    limits: {
      fileSize: 25 * 1024 * 1024, // Limite de 25MB por foto de alta resolução
      files: 10
    }
  });

  // 4. Servidor de WebSockets para atualizações em tempo real
  await server.register(fastifyWebsocket);

  server.get('/ws', { websocket: true }, (socket, _req) => {
    wsHub.register(socket);
  });

  // 5. Servidor estático para entrega de fotos originais, comprimidas e thumbnails
  await server.register(fastifyStatic, {
    root: path.resolve(env.STORAGE_DIR),
    prefix: '/uploads/'
  });

  // 6. Registra rotas da aplicação
  await server.register(healthRoutes);
  await server.register(uploadRoutes);
  await server.register(vistoriaRoutes);

  return server;
}
