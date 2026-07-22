namespace Slovarik.Application.DTOs;

public record UserDto(
    Guid Id,
    long TelegramId,
    string? UserName,
    string? FullName,
    string? PhoneNumber,
    DateTimeOffset CreatedAt,
    int WordCount);
