using Slovarik.Application.DTOs;
using Slovarik.Application.Interfaces;

namespace Slovarik.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories");

        group.MapGet("/by-user/{userId:guid}", async (Guid userId, ICategoryService categoryService, CancellationToken ct) =>
        {
            var categories = await categoryService.GetByUserIdAsync(userId, ct);
            return Results.Ok(categories);
        })
        .WithName("GetCategoriesByUserId")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (Guid id, ICategoryService categoryService, CancellationToken ct) =>
        {
            var category = await categoryService.GetByIdAsync(id, ct);
            return category is null
                ? Results.NotFound(new ErrorResponseDto("Kategoriya topilmadi."))
                : Results.Ok(category);
        })
        .WithName("GetCategoryById")
        .WithOpenApi();

        group.MapPost("/", async (Guid userId, CategoryCreateDto dto, ICategoryService categoryService, CancellationToken ct) =>
        {
            var category = await categoryService.CreateAsync(userId, dto, ct);
            return Results.Created($"/api/categories/{category.Id}", category);
        })
        .WithName("CreateCategory")
        .WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, CategoryUpdateDto dto, ICategoryService categoryService, CancellationToken ct) =>
        {
            var category = await categoryService.UpdateAsync(id, dto, ct);
            return category is null
                ? Results.NotFound(new ErrorResponseDto("Kategoriya topilmadi."))
                : Results.Ok(category);
        })
        .WithName("UpdateCategory")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, ICategoryService categoryService, CancellationToken ct) =>
        {
            var deleted = await categoryService.DeleteAsync(id, ct);
            return deleted
                ? Results.NoContent()
                : Results.NotFound(new ErrorResponseDto("Kategoriya topilmadi."));
        })
        .WithName("DeleteCategory")
        .WithOpenApi();
    }
}
