namespace Slovarik.Application.DTOs;

public record ErrorResponseDto(string Error, string? Details = null);
