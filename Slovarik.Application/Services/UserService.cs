using Slovarik.Application.DTOs;
using Slovarik.Application.Interfaces;
using Slovarik.Domain.Entities;
using Slovarik.Domain.Interfaces;

namespace Slovarik.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> GetOrCreateUserAsync(long telegramId, string? userName, string? fullName, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId, ct);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                TelegramId = telegramId,
                UserName = userName,
                FullName = fullName
            };
            await _userRepository.AddAsync(user, ct);
            await _userRepository.SaveChangesAsync(ct);
            return MapToDto(user, 0);
        }

        var wordCount = await _userRepository.GetWordCountAsync(user.Id, ct);
        return MapToDto(user, wordCount);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null)
            return null;

        var words = await _userRepository.GetWordCountAsync(id, ct);
        return MapToDto(user, words);
    }

    public async Task<UserDto?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId, ct);
        if (user is null)
            return null;

        var words = await _userRepository.GetWordCountAsync(user.Id, ct);
        return MapToDto(user, words);
    }

    private static UserDto MapToDto(User user, int wordCount)
    {
        return new UserDto(
            user.Id,
            user.TelegramId,
            user.UserName,
            user.FullName,
            user.PhoneNumber,
            user.CreatedAt,
            wordCount);
    }
}
