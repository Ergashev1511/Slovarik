using Microsoft.EntityFrameworkCore;
using Slovarik.Domain.Entities;
using Slovarik.Domain.Interfaces;
using Slovarik.Infrastructure.Data;

namespace Slovarik.Infrastructure.Repositories;

public class WordRepository : IWordRepository
{
    private readonly SlovarikDbContext _context;

    public WordRepository(SlovarikDbContext context)
    {
        _context = context;
    }

    public async Task<List<Word>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Words
            .Include(w => w.Category)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Word>> GetByUserIdAndCategoryAsync(Guid userId, Guid categoryId, CancellationToken ct = default)
    {
        return await _context.Words
            .Include(w => w.Category)
            .Where(w => w.UserId == userId && w.CategoryId == categoryId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<Word> words, CancellationToken ct = default)
    {
        await _context.Words.AddRangeAsync(words, ct);
    }

    public async Task<List<Word>> GetRandomByUserIdAsync(Guid userId, int count, CancellationToken ct = default)
    {
        return await _context.Words
            .Where(w => w.UserId == userId)
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
