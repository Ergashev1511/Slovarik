using Slovarik.Application.DTOs;

namespace Slovarik.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> GetOrCreateUserAsync(long telegramId, string? userName, string? fullName, CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserDto?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default);
}
