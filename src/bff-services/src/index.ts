import { env } from './config/env.js';
import { createServer } from './server.js';

async function bootstrap(): Promise<void> {
  const server = await createServer();
  try {
    await server.listen({ port: env.PORT, host: env.HOST });
    console.log(`🚀 [BFF Fastify] Servidor ativo em http://${env.HOST}:${env.PORT}`);
    console.log(`⚡ [WebSocket Gateway] ws://${env.HOST}:${env.PORT}/ws`);
  } catch (err) {
    server.log.error(err);
    process.exit(1);
  }
}

bootstrap();
