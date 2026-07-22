using Microsoft.EntityFrameworkCore;
using Slovarik.Domain.Entities;

namespace Slovarik.Infrastructure.Data;

public class SlovarikDbContext : DbContext
{
    public SlovarikDbContext(DbContextOptions<SlovarikDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Word> Words => Set<Word>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TelegramId).IsRequired();
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.HasIndex(e => e.TelegramId).IsUnique();
        });

        modelBuilder.Entity<Word>(entity =>
        {
            entity.ToTable("words");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WordUz).IsRequired().HasMaxLength(500);
            entity.Property(e => e.WordRu).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Words)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
