export enum TipoAcaoAvaria {
  Recuperar = 1,
  Substituir = 2,
  Pintura = 3
}

export interface ItemAvaria {
  id?: string;
  peca: string;
  tipoAcao: TipoAcaoAvaria;
  custoMaoDeObra: number;
  custoPecas: number;
}

export interface FotoEvidencia {
  id: string;
  originalFileName: string;
  originalUrl: string;
  compressedUrl: string;
  thumbnailUrl: string;
  hashArquivoSha256: string;
  originalSizeBytes: number;
  compressedSizeBytes: number;
  taxaCompressaoPercentual: number;
  largura: number;
  altura: number;
  latitude?: number;
  longitude?: number;
  dataCapturaExif?: string;
}

export interface VistoriaPendente {
  vistoriaId: string;
  sinistroId: string;
  numeroSinistro: string;
  veiculo: string;
  placa: string;
  dataAbertura: string;
  status: string;
  franquia: number;
  fipe: number;
}

export interface ConcluirVistoriaRequest {
  sinistroId: string;
  peritoId: string;
  observacoesPerito: string;
  avarias: {
    peca: string;
    tipoAcao: number;
    custoMaoDeObra: number;
    custoPecas: number;
  }[];
  evidencias: {
    urlOriginal: string;
    urlComprimida: string;
    hashArquivoSha256: string;
    latitude?: number;
    longitude?: number;
    dataCapturaExif?: string;
  }[];
}
