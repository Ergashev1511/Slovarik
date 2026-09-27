namespace Slovarik.Application.DTOs;

public record WordDto(Guid Id, string WordUz, string WordRu, Guid? CategoryId, string? CategoryName, DateTimeOffset CreatedAt);

public record BatchWordDto(string WordUz, string WordRu);
public record BatchInsertRequestDto(Guid UserId, Guid? CategoryId, List<BatchWordDto> Words);
public record BatchInsertResultDto(bool Success, string Message, int Count);

// Legacy: kept for IAIService / OpenAIService — do not remove
public record WordPairDto(string WordUz, string WordRu);

// Legacy: AI-based upload result — kept for future reactivation
// public record UploadResultDto(bool Success, string Message, List<WordPairDto> Words);
