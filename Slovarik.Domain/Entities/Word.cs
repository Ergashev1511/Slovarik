namespace Slovarik.Domain.Entities;

public class Word
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string WordUz { get; set; } = null!;
    public string WordRu { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }
}
