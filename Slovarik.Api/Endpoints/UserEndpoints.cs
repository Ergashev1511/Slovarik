using Slovarik.Application.DTOs;
using Slovarik.Application.Interfaces;

namespace Slovarik.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users");

        group.MapGet("/{userId:guid}", async (Guid userId, IUserService userService, CancellationToken ct) =>
        {
            var user = await userService.GetByIdAsync(userId, ct);
            return user is null
                ? Results.NotFound(new ErrorResponseDto("Foydalanuvchi topilmadi."))
                : Results.Ok(user);
        })
        .WithName("GetUserById")
        .WithOpenApi();

        group.MapGet("/by-telegram/{telegramId:long}", async (long telegramId, IUserService userService, CancellationToken ct) =>
        {
            var user = await userService.GetByTelegramIdAsync(telegramId, ct);
            return user is null
                ? Results.NotFound(new ErrorResponseDto("Foydalanuvchi topilmadi."))
                : Results.Ok(user);
        })
        .WithName("GetUserByTelegramId")
        .WithOpenApi();
    }
}
