import dotenv from 'dotenv';
import { z } from 'zod';

dotenv.config();

const envSchema = z.object({
  PORT: z.string().default('3000').transform(Number),
  HOST: z.string().default('0.0.0.0'),
  RABBITMQ_URL: z.string().default('amqp://guest:guest@localhost:5672'),
  RABBITMQ_EXCHANGE: z.string().default('sinistros.exchange'),
  STORAGE_DIR: z.string().default('./uploads'),
  DOTNET_API_URL: z.string().default('http://localhost:5000')
});

export const env = envSchema.parse(process.env);
