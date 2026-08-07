using Slovarik.Application.DTOs;

namespace Slovarik.Application.Interfaces;

public interface IAIService
{
    Task<List<WordPairDto>> ExtractWordsFromImageAsync(byte[] imageBytes, string contentType, CancellationToken ct = default);
    Task<List<WordPairDto>> ExtractWordsFromPdfAsync(byte[] pdfBytes, CancellationToken ct = default);
}
