using Slovarik.Application.DTOs;
using Slovarik.Application.Interfaces;

namespace Slovarik.Api.Endpoints;

public static class WordEndpoints
{
    private const long MaxFileSize = 20 * 1024 * 1024; // 20MB

    public static void MapWordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/words");

        group.MapGet("/{userId:guid}", async (Guid userId, IWordService wordService, CancellationToken ct) =>
        {
            var words = await wordService.GetByUserIdAsync(userId, ct);
            return Results.Ok(words);
        })
        .WithName("GetWordsByUserId")
        .WithOpenApi();

        group.MapPost("/upload", async (HttpRequest request, IWordService wordService, CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest(new ErrorResponseDto("Form content kutilmoqda."));

            var form = await request.ReadFormAsync(ct);
            var userIdStr = form["userId"].ToString();
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.BadRequest(new ErrorResponseDto("Noto'g'ri userId."));

            Guid? categoryId = null;
            var categoryIdStr = form["categoryId"].ToString();
            if (!string.IsNullOrWhiteSpace(categoryIdStr) && Guid.TryParse(categoryIdStr, out var parsedCategoryId))
                categoryId = parsedCategoryId;

            var file = form.Files.FirstOrDefault();
            if (file is null || file.Length == 0)
                return Results.BadRequest(new ErrorResponseDto("Fayl kerak."));

            if (file.Length > MaxFileSize)
                return Results.BadRequest(new ErrorResponseDto("Fayl hajmi 20MB dan oshmasligi kerak."));

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            var fileBytes = ms.ToArray();

            var result = await wordService.UploadAndProcessAsync(userId, categoryId, fileBytes, file.ContentType, file.FileName, ct);
            return Results.Ok(result);
        })
        .WithName("UploadWords")
        .WithOpenApi();
    }
}
