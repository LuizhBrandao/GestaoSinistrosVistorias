import { FastifyInstance, FastifyPluginAsync } from 'fastify';
import { z } from 'zod';
import { rabbitMq } from '../services/rabbitmqService.js';
import { wsHub } from '../services/websocketHub.js';

const ConcluirVistoriaSchema = z.object({
  sinistroId: z.string().uuid(),
  peritoId: z.string().min(3),
  observacoesPerito: z.string().min(5),
  avarias: z.array(
    z.object({
      peca: z.string(),
      tipoAcao: z.number().int().min(1).max(3),
      custoMaoDeObra: z.number().min(0),
      custoPecas: z.number().min(0)
    })
  ).min(1, 'Pelo menos uma avaria deve ser registrada.'),
  evidencias: z.array(
    z.object({
      urlOriginal: z.string(),
      urlComprimida: z.string(),
      hashArquivoSha256: z.string().min(32),
      latitude: z.number().optional(),
      longitude: z.number().optional(),
      dataCapturaExif: z.string().optional()
    })
  ).min(1, 'Ao menos uma foto é obrigatória para conclusão do laudo.')
});

export const vistoriaRoutes: FastifyPluginAsync = async (server: FastifyInstance) => {
  // Lista de vistorias pendentes para o aplicativo do perito
  server.get('/api/vistorias/pendentes', async (_request, reply) => {
    return reply.send([
      {
        vistoriaId: 'a3d4e5f6-7890-4123-8abc-def012345678',
        sinistroId: '11111111-1111-1111-1111-111111111111',
        numeroSinistro: 'SIN-2026-0001',
        veiculo: 'Toyota Corolla Altis 2.0',
        placa: 'BRA-2E19',
        dataAbertura: new Date().toISOString(),
        status: 'AguardandoVistoria',
        franquia: 2500.0,
        fipe: 125000.0
      }
    ]);
  });

  // Concluir vistoria e despachar evento assíncrono para regulação .NET
  server.post('/api/vistorias/:id/concluir', async (request, reply) => {
    const { id } = request.params as { id: string };

    const parseResult = ConcluirVistoriaSchema.safeParse(request.body);
    if (!parseResult.success) {
      return reply.status(400).send({
        erro: 'Dados inválidos para conclusão do laudo pericial.',
        detalhes: parseResult.error.errors
      });
    }

    const payload = parseResult.data;
    const custoTotal = payload.avarias.reduce(
      (acc, item) => acc + item.custoMaoDeObra + item.custoPecas,
      0
    );

    // 1. Notifica via WebSocket que a vistoria foi transmitida com sucesso
    wsHub.broadcast('VISTORIA_CONCLUIDA', {
      vistoriaId: id,
      sinistroId: payload.sinistroId,
      peritoId: payload.peritoId,
      custoTotalOrcado: custoTotal,
      quantidadeAvarias: payload.avarias.length,
      quantidadeFotos: payload.evidencias.length,
      dataConclusaoUtc: new Date().toISOString()
    });

    // 2. Publica evento no RabbitMQ para o motor de regulação em .NET 10
    const publicadoRabbit = await rabbitMq.publish('vistoria.evento.concluida', {
      vistoriaId: id,
      sinistroId: payload.sinistroId,
      peritoId: payload.peritoId,
      custoTotalOrcado: custoTotal,
      quantidadeAvarias: payload.avarias.length,
      quantidadeFotos: payload.evidencias.length,
      dataConclusaoUtc: new Date().toISOString()
    });

    return reply.status(200).send({
      sucesso: true,
      mensagem: 'Laudo pericial submetido e despachado para a esteira de regulação.',
      vistoriaId: id,
      custoTotalOrcado: custoTotal,
      enviadoAoBroker: publicadoRabbit
    });
  });
};
