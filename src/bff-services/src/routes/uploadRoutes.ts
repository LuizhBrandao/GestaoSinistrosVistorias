import { FastifyInstance, FastifyPluginAsync } from 'fastify';
import { imageProcessor } from '../services/imageProcessor.js';
import { wsHub } from '../services/websocketHub.js';

export const uploadRoutes: FastifyPluginAsync = async (server: FastifyInstance) => {
  server.post('/api/vistorias/upload', async (request, reply) => {
    try {
      const data = await request.file();

      if (!data) {
        return reply.status(400).send({
          erro: 'Nenhum arquivo de imagem foi enviado no corpo da requisição.'
        });
      }

      const buffer = await data.toBuffer();
      const originalName = data.filename || 'foto-vistoria.jpg';

      // Processa a imagem (compressão WebP, hash SHA-256 e thumbnail)
      const resultado = await imageProcessor.processImage(buffer, originalName);

      // Notifica todos os peritos conectados no WebSocket instantaneamente
      wsHub.broadcast('FOTO_PROCESSADA', {
        id: resultado.id,
        nome: resultado.originalFileName,
        urlThumb: resultado.thumbnailUrl,
        taxaCompressao: `${resultado.taxaCompressaoPercentual}%`,
        hashSha256: resultado.hashArquivoSha256,
        latitude: resultado.latitude,
        longitude: resultado.longitude
      });

      return reply.status(201).send(resultado);
    } catch (error) {
      request.log.error(error, 'Erro ao processar upload de foto');
      return reply.status(500).send({
        erro: 'Falha interna durante a compressão e processamento da evidência fotográfica.'
      });
    }
  });
};
