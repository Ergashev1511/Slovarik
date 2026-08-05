using Microsoft.EntityFrameworkCore;
using Slovarik.Domain.Entities;
using Slovarik.Domain.Interfaces;
using Slovarik.Infrastructure.Data;

namespace Slovarik.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly SlovarikDbContext _context;

    public CategoryRepository(SlovarikDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Categories
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<int> GetWordCountAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await _context.Words.CountAsync(w => w.CategoryId == categoryId, ct);
    }

    public async Task<Category> AddAsync(Category category, CancellationToken ct = default)
    {
        await _context.Categories.AddAsync(category, ct);
        return category;
    }

    public Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        _context.Categories.Update(category);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Category category, CancellationToken ct = default)
    {
        _context.Categories.Remove(category);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
