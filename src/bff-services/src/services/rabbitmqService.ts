import amqp, { Channel } from 'amqplib';
import { env } from '../config/env.js';
import { wsHub } from './websocketHub.js';

type AmqpConnection = Awaited<ReturnType<typeof amqp.connect>>;

export class RabbitMqService {
  private connection: AmqpConnection | null = null;
  private channel: Channel | null = null;
  private isConnecting = false;

  async init(): Promise<void> {
    if (this.isConnecting) return;
    this.isConnecting = true;

    try {
      this.connection = await amqp.connect(env.RABBITMQ_URL);
      const ch = await this.connection.createChannel();
      this.channel = ch;

      await ch.assertExchange(env.RABBITMQ_EXCHANGE, 'topic', { durable: true });

      // Fila exclusiva do BFF para ouvir eventos do .NET
      const q = await ch.assertQueue('bff.vistorias.eventos', { durable: true });
      await ch.bindQueue(q.queue, env.RABBITMQ_EXCHANGE, 'sinistro.evento.*');
      await ch.bindQueue(q.queue, env.RABBITMQ_EXCHANGE, 'vistoria.evento.*');

      // Consome eventos e repassa via WebSocket para os peritos no Angular
      await ch.consume(q.queue, (msg) => {
        if (!msg) return;

        try {
          const content = JSON.parse(msg.content.toString());
          const routingKey = msg.fields.routingKey;

          // Propaga evento para o Angular
          wsHub.broadcast('EVENTO_DOMINIO_RECEBIDO', {
            routingKey,
            data: content
          });

          ch.ack(msg);
        } catch (err) {
          console.error('[RabbitMQ] Erro ao processar mensagem recebida:', err);
          ch.nack(msg, false, false);
        }
      });

      console.log(`[RabbitMQ] Conectado e ouvindo tópicos em ${env.RABBITMQ_EXCHANGE}`);
    } catch (err) {
      console.warn('[RabbitMQ] Broker offline ou iniciando. O BFF continuará operando com fallback local.');
    } finally {
      this.isConnecting = false;
    }
  }

  async publish(routingKey: string, payload: unknown): Promise<boolean> {
    try {
      if (!this.channel) {
        await this.init();
      }

      if (this.channel) {
        const buffer = Buffer.from(JSON.stringify(payload));
        return this.channel.publish(env.RABBITMQ_EXCHANGE, routingKey, buffer, {
          persistent: true,
          contentType: 'application/json',
          timestamp: Date.now()
        });
      }
      return false;
    } catch (err) {
      console.warn(`[RabbitMQ] Falha ao publicar mensagem em ${routingKey}:`, err);
      return false;
    }
  }
}

export const rabbitMq = new RabbitMqService();
