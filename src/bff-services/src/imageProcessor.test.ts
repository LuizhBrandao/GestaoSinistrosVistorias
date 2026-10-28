import assert from 'node:assert';
import test from 'node:test';
import sharp from 'sharp';
import { imageProcessor } from './services/imageProcessor.js';

test('ImageProcessorService: deve processar, comprimir em WebP e calcular hash SHA-256', async () => {
  await imageProcessor.init();

  // Cria uma imagem JPEG de teste 400x300 em memória
  const bufferOriginal = await sharp({
    create: {
      width: 400,
      height: 300,
      channels: 3,
      background: { r: 255, g: 0, b: 0 }
    }
  }).jpeg().toBuffer();

  const resultado = await imageProcessor.processImage(bufferOriginal, 'teste_avaria.jpg');

  assert.ok(resultado.id, 'O ID da imagem deve ser gerado');
  assert.equal(resultado.hashArquivoSha256.length, 64, 'Hash SHA-256 deve ter 64 caracteres hexadecimais');
  assert.ok(resultado.compressedUrl.endsWith('.webp'), 'Arquivo comprimido deve estar em formato WebP');
  assert.ok(resultado.thumbnailUrl.endsWith('.webp'), 'Thumbnail deve estar em formato WebP');
  assert.equal(resultado.largura, 400, 'Largura original deve ser 400');
  assert.equal(resultado.altura, 300, 'Altura original deve ser 300');
});
