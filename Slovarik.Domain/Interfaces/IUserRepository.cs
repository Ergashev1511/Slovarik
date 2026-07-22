using Slovarik.Domain.Entities;

namespace Slovarik.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User> AddAsync(User user, CancellationToken ct = default);
    Task<int> GetWordCountAsync(Guid userId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
