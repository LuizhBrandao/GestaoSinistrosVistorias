namespace Sinistros.Domain.Enums;

public enum StatusSinistro
{
    Aberto = 1,
    AguardandoVistoria = 2,
    VistoriaConcluida = 3,
    EmRegulacao = 4,
    AguardandoAprovacaoAlcada = 5,
    Aprovado = 6,
    Liquidado = 7,
    Rejeitado = 8
}

public enum StatusVistoria
{
    Agendada = 1,
    EmAndamento = 2,
    Concluida = 3,
    Cancelada = 4
}

public enum TipoPerda
{
    NaoAvaliada = 0,
    PerdaParcial = 1,
    PerdaTotal = 2
}

public enum NivelAlcada
{
    Operacional = 1, // Limite até R$ 10.000,00
    Supervisao = 2,  // Limite até R$ 50.000,00
    Gerencial = 3,   // Limite acima de R$ 50.000,00 ou Perda Total
    Diretoria = 4    // Ilimitado
}

public enum TipoAcaoAvaria
{
    Recuperar = 1,
    Substituir = 2,
    Pintura = 3
}
