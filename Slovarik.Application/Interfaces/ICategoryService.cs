using Slovarik.Application.DTOs;

namespace Slovarik.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(Guid userId, CategoryCreateDto dto, CancellationToken ct = default);
    Task<CategoryDto?> UpdateAsync(Guid id, CategoryUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
