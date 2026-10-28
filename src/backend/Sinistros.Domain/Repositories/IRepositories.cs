using Sinistros.Domain.Aggregates;

namespace Sinistros.Domain.Repositories;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}

public interface ISinistroRepository
{
    Task<Sinistro?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sinistro?> ObterPorNumeroAsync(string numeroSinistro, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sinistro>> ObterPendentesAprovacaoAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Sinistro sinistro, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Sinistro sinistro, CancellationToken cancellationToken = default);
}

public interface IVistoriaRepository
{
    Task<Vistoria?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vistoria?> ObterPorSinistroIdAsync(Guid sinistroId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Vistoria vistoria, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Vistoria vistoria, CancellationToken cancellationToken = default);
}
