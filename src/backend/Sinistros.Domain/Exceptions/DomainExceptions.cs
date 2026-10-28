namespace Sinistros.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class RegraNegocioException : DomainException
{
    public string CodigoRegra { get; }

    public RegraNegocioException(string codigoRegra, string message) : base(message)
    {
        CodigoRegra = codigoRegra;
    }
}

public class AlcadaInsuficienteException : DomainException
{
    public decimal ValorRequerido { get; }
    public decimal LimiteDisponivel { get; }
    public string CargoUsuario { get; }

    public AlcadaInsuficienteException(string cargoUsuario, decimal valorRequerido, decimal limiteDisponivel)
        : base($"O usuário com alçada '{cargoUsuario}' possui limite de R$ {limiteDisponivel:N2}, insuficiente para aprovar R$ {valorRequerido:N2}.")
    {
        CargoUsuario = cargoUsuario;
        ValorRequerido = valorRequerido;
        LimiteDisponivel = limiteDisponivel;
    }
}
