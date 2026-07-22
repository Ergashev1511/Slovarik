using Microsoft.EntityFrameworkCore;
using Slovarik.Domain.Entities;
using Slovarik.Domain.Interfaces;
using Slovarik.Infrastructure.Data;

namespace Slovarik.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SlovarikDbContext _context;

    public UserRepository(SlovarikDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId, ct);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
        return user;
    }

    public async Task<int> GetWordCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Words.CountAsync(w => w.UserId == userId, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
