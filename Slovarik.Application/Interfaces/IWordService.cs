using Slovarik.Application.DTOs;

namespace Slovarik.Application.Interfaces;

public interface IWordService
{
    Task<List<WordDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<BatchInsertResultDto> BatchInsertAsync(BatchInsertRequestDto request, CancellationToken ct = default);

    // Legacy: AI-based file upload — kept for future reactivation
    // Task<UploadResultDto> UploadAndProcessAsync(Guid userId, Guid? categoryId, byte[] fileBytes, string contentType, string fileName, CancellationToken ct = default);
}
