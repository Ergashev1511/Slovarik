namespace Slovarik.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public long  TelegramId { get; set; }
    public string? UserName { get; set; }
    public string ? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Word> Words { get; set; } = new List<Word>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}