namespace Sinistros.Application.Common;

/// <summary>
/// Representa o resultado de uma operação na camada de aplicação.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string? error = null, string? errorCode = null)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("Um resultado de sucesso não pode conter mensagem de erro.");
        if (!isSuccess && error is null)
            throw new InvalidOperationException("Um resultado de falha deve conter mensagem de erro.");

        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true);
    public static Result Failure(string error, string? errorCode = null) => new(false, error, errorCode);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Não é possível acessar o valor de um resultado com falha.");

    private Result(T value) : base(true)
    {
        _value = value;
    }

    private Result(string error, string? errorCode = null) : base(false, error, errorCode)
    {
        _value = default;
    }

    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(string error, string? errorCode = null) => new(error, errorCode);
}
