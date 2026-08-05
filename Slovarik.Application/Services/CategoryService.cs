using Slovarik.Application.DTOs;
using Slovarik.Application.Interfaces;
using Slovarik.Domain.Entities;
using Slovarik.Domain.Interfaces;

namespace Slovarik.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var categories = await _categoryRepository.GetByUserIdAsync(userId, ct);
        var dtos = new List<CategoryDto>();
        foreach (var c in categories)
        {
            var wordCount = await _categoryRepository.GetWordCountAsync(c.Id, ct);
            dtos.Add(MapToDto(c, wordCount));
        }
        return dtos;
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, ct);
        if (category is null)
            return null;

        var count = await _categoryRepository.GetWordCountAsync(id, ct);
        return MapToDto(category, count);
    }

    public async Task<CategoryDto> CreateAsync(Guid userId, CategoryCreateDto dto, CancellationToken ct = default)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            UserId = userId
        };

        await _categoryRepository.AddAsync(category, ct);
        await _categoryRepository.SaveChangesAsync(ct);
        return MapToDto(category, 0);
    }

    public async Task<CategoryDto?> UpdateAsync(Guid id, CategoryUpdateDto dto, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, ct);
        if (category is null)
            return null;

        category.Name = dto.Name;
        await _categoryRepository.UpdateAsync(category, ct);
        await _categoryRepository.SaveChangesAsync(ct);

        var count = await _categoryRepository.GetWordCountAsync(id, ct);
        return MapToDto(category, count);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, ct);
        if (category is null)
            return false;

        await _categoryRepository.DeleteAsync(category, ct);
        await _categoryRepository.SaveChangesAsync(ct);
        return true;
    }

    private static CategoryDto MapToDto(Category category, int wordCount)
    {
        return new CategoryDto(category.Id, category.Name, category.UserId, category.CreatedAt, wordCount);
    }
}
