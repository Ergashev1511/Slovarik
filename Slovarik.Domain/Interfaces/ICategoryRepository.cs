using Slovarik.Domain.Entities;

namespace Slovarik.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<int> GetWordCountAsync(Guid categoryId, CancellationToken ct = default);
    Task<Category> AddAsync(Category category, CancellationToken ct = default);
    Task UpdateAsync(Category category, CancellationToken ct = default);
    Task DeleteAsync(Category category, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
