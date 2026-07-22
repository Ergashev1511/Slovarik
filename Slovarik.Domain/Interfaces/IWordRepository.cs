using Slovarik.Domain.Entities;

namespace Slovarik.Domain.Interfaces;

public interface IWordRepository
{
    Task<List<Word>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Word> words, CancellationToken ct = default);
    Task<List<Word>> GetRandomByUserIdAsync(Guid userId, int count, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
