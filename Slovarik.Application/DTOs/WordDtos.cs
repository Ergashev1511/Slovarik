namespace Slovarik.Application.DTOs;

public record WordDto(Guid Id, string WordUz, string WordRu, Guid? CategoryId, string? CategoryName, DateTimeOffset CreatedAt);

public record WordPairDto(string WordUz, string WordRu);

public record UploadResultDto(bool Success, string Message, List<WordPairDto> Words);
