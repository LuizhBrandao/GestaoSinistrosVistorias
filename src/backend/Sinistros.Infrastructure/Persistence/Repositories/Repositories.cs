using Microsoft.EntityFrameworkCore;
using Sinistros.Domain.Aggregates;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Repositories;

namespace Sinistros.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SinistrosDbContext _context;

    public UnitOfWork(SinistrosDbContext context)
    {
        _context = context;
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}

public sealed class SinistroRepository : ISinistroRepository
{
    private readonly SinistrosDbContext _context;

    public SinistroRepository(SinistrosDbContext context)
    {
        _context = context;
    }

    public async Task<Sinistro?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sinistros
            .Include(s => s.Parecer)
            .Include(s => s.Auditoria)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sinistro?> ObterPorNumeroAsync(string numeroSinistro, CancellationToken cancellationToken = default)
    {
        return await _context.Sinistros
            .Include(s => s.Parecer)
            .Include(s => s.Auditoria)
            .FirstOrDefaultAsync(s => s.NumeroSinistro == numeroSinistro.ToUpperInvariant(), cancellationToken);
    }

    public async Task<IReadOnlyList<Sinistro>> ObterPendentesAprovacaoAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sinistros
            .Include(s => s.Parecer)
            .Where(s => s.Status == StatusSinistro.AguardandoAprovacaoAlcada || s.Status == StatusSinistro.Aberto || s.Status == StatusSinistro.AguardandoVistoria)
            .OrderByDescending(s => s.DataAberturaUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Sinistro sinistro, CancellationToken cancellationToken = default)
    {
        await _context.Sinistros.AddAsync(sinistro, cancellationToken);
    }

    public Task AtualizarAsync(Sinistro sinistro, CancellationToken cancellationToken = default)
    {
        _context.Sinistros.Update(sinistro);
        return Task.CompletedTask;
    }
}

public sealed class VistoriaRepository : IVistoriaRepository
{
    private readonly SinistrosDbContext _context;

    public VistoriaRepository(SinistrosDbContext context)
    {
        _context = context;
    }

    public async Task<Vistoria?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Vistorias
            .Include(v => v.Avarias)
            .Include(v => v.Evidencias)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Vistoria?> ObterPorSinistroIdAsync(Guid sinistroId, CancellationToken cancellationToken = default)
    {
        return await _context.Vistorias
            .Include(v => v.Avarias)
            .Include(v => v.Evidencias)
            .OrderByDescending(v => v.DataAgendamentoUtc)
            .FirstOrDefaultAsync(v => v.SinistroId == sinistroId, cancellationToken);
    }

    public async Task AdicionarAsync(Vistoria vistoria, CancellationToken cancellationToken = default)
    {
        await _context.Vistorias.AddAsync(vistoria, cancellationToken);
    }

    public Task AtualizarAsync(Vistoria vistoria, CancellationToken cancellationToken = default)
    {
        _context.Vistorias.Update(vistoria);
        return Task.CompletedTask;
    }
}
