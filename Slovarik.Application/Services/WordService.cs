using Slovarik.Application.DTOs;
using Slovarik.Application.Interfaces;
using Slovarik.Domain.Entities;
using Slovarik.Domain.Interfaces;

namespace Slovarik.Application.Services;

public class WordService : IWordService
{
    private readonly IWordRepository _wordRepository;
    private readonly IAIService _aiService;

    public WordService(IWordRepository wordRepository, IAIService aiService)
    {
        _wordRepository = wordRepository;
        _aiService = aiService;
    }

    public async Task<List<WordDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var words = await _wordRepository.GetByUserIdAsync(userId, ct);
        return words.Select(MapToDto).ToList();
    }

    public async Task<BatchInsertResultDto> BatchInsertAsync(BatchInsertRequestDto request, CancellationToken ct = default)
    {
        if (request.Words == null || request.Words.Count == 0)
            return new BatchInsertResultDto(false, "So'zlar ro'yxati bo'sh.", 0);

        var valid = request.Words
            .Where(w => !string.IsNullOrWhiteSpace(w.WordUz) && !string.IsNullOrWhiteSpace(w.WordRu))
            .ToList();

        if (valid.Count == 0)
            return new BatchInsertResultDto(false, "Yaroqli so'z juftliklari topilmadi.", 0);

        var words = valid.Select(w => new Word
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            WordUz = w.WordUz.Trim(),
            WordRu = w.WordRu.Trim(),
            CategoryId = request.CategoryId
        }).ToList();

        await _wordRepository.AddRangeAsync(words, ct);
        await _wordRepository.SaveChangesAsync(ct);

        return new BatchInsertResultDto(true, $"{valid.Count} ta so'z saqlandi.", valid.Count);
    }

    // ── Legacy: AI-based upload — kept for future reactivation ───────────────────────────────
    // public async Task<UploadResultDto> UploadAndProcessAsync(
    //     Guid userId,
    //     Guid? categoryId,
    //     byte[] fileBytes,
    //     string contentType,
    //     string fileName,
    //     CancellationToken ct = default)
    // {
    //     List<WordPairDto> pairs;
    //
    //     if (contentType.StartsWith("image/"))
    //     {
    //         pairs = await _aiService.ExtractWordsFromImageAsync(fileBytes, contentType, ct);
    //     }
    //     else if (contentType is "application/pdf" || fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
    //     {
    //         pairs = await _aiService.ExtractWordsFromPdfAsync(fileBytes, ct);
    //     }
    //     else
    //     {
    //         return new UploadResultDto(false, "Faqat rasm yoki PDF fayl yuklash mumkin.", new List<WordPairDto>());
    //     }
    //
    //     if (pairs.Count == 0)
    //     {
    //         return new UploadResultDto(false, "So'zlar topilmadi.", new List<WordPairDto>());
    //     }
    //
    //     var words = pairs.Select(p => new Word
    //     {
    //         Id = Guid.NewGuid(),
    //         UserId = userId,
    //         WordUz = p.WordUz,
    //         WordRu = p.WordRu,
    //         CategoryId = categoryId
    //     }).ToList();
    //
    //     await _wordRepository.AddRangeAsync(words, ct);
    //     await _wordRepository.SaveChangesAsync(ct);
    //
    //     return new UploadResultDto(true, $"{pairs.Count} ta so'z saqlandi.", pairs);
    // }
    // ─────────────────────────────────────────────────────────────────────────────────────────

    private static WordDto MapToDto(Word word)
    {
        return new WordDto(word.Id, word.WordUz, word.WordRu, word.CategoryId, word.Category?.Name, word.CreatedAt);
    }
}
