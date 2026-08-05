namespace Slovarik.Application.DTOs;

public record CategoryDto(Guid Id, string Name, Guid UserId, DateTimeOffset CreatedAt, int WordCount);

public record CategoryCreateDto(string Name);

public record CategoryUpdateDto(string Name);
