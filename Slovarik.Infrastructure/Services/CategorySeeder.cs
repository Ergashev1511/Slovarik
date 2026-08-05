using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Slovarik.Infrastructure.Data;

namespace Slovarik.Infrastructure.Services;

public static class CategorySeeder
{
    public static async Task SeedAsync(SlovarikDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var seedPath = Path.Combine(AppContext.BaseDirectory, "seed-categories.txt");
        if (!File.Exists(seedPath))
        {
            seedPath = "seed-categories.txt";
            if (!File.Exists(seedPath))
                return;
        }

        var categoryNames = await File.ReadAllLinesAsync(seedPath, ct);
        categoryNames = categoryNames.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).ToArray();

        if (categoryNames.Length == 0)
            return;

        var userIds = await db.Users.Select(u => u.Id).ToListAsync(ct);
        if (userIds.Count == 0)
            return;

        var existing = await db.Categories
            .Select(c => new { c.UserId, c.Name })
            .ToListAsync(ct);

        var existingSet = existing
            .Select(c => $"{c.UserId}:{c.Name.Trim().ToLowerInvariant()}")
            .ToHashSet();

        foreach (var userId in userIds)
        {
            foreach (var name in categoryNames)
            {
                var key = $"{userId}:{name.Trim().ToLowerInvariant()}";
                if (existingSet.Contains(key))
                    continue;

                db.Categories.Add(new Domain.Entities.Category
                {
                    Id = Guid.NewGuid(),
                    Name = name.Trim(),
                    UserId = userId,
                    CreatedAt = DateTimeOffset.UtcNow
                });
                existingSet.Add(key);
            }
        }

        var added = await db.SaveChangesAsync(ct);
        if (added > 0)
            logger.LogInformation("Seed: {Count} ta kategoriya qo'shildi.", added);
    }
}
