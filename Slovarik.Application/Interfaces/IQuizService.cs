using Slovarik.Application.DTOs;

namespace Slovarik.Application.Interfaces;

public interface IQuizService
{
    Task<QuizStartResponseDto> StartQuizAsync(Guid userId, int questionCount, QuizDirection direction, bool selectFromEnd, Guid? categoryId, CancellationToken ct = default);
}
