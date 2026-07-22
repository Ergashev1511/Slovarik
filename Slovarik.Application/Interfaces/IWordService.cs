using Slovarik.Application.DTOs;

namespace Slovarik.Application.Interfaces;

public interface IWordService
{
    Task<List<WordDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<UploadResultDto> UploadAndProcessAsync(Guid userId, byte[] fileBytes, string contentType, string fileName, CancellationToken ct = default);
}
