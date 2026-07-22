using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Slovarik.Application.DTOs;
using Slovarik.Application.Interfaces;
using Slovarik.Domain.Interfaces;
using Slovarik.Application.Settings;

namespace Slovarik.Application.Services;

public class QuizService : IQuizService
{
    private readonly IWordRepository _wordRepository;
    private readonly QuizSettings _settings;

    public QuizService(IWordRepository wordRepository, IOptions<QuizSettings> settings)
    {
        _wordRepository = wordRepository;
        _settings = settings.Value;
    }

    public async Task<QuizStartResponseDto> StartQuizAsync(
        Guid userId,
        int questionCount,
        QuizDirection direction,
        bool selectFromEnd,
        CancellationToken ct = default)
    {
        var words = await _wordRepository.GetByUserIdAsync(userId, ct);

        if (words.Count < 4)
            throw new InvalidOperationException("Quiz boshlash uchun kamida 4 ta so'z kerak.");

        var count = Math.Min(questionCount, words.Count);

        var selectedWords = selectFromEnd
            ? words.Take(count).ToList()
            : words.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();

        var questions = selectedWords.Select((w, i) =>
        {
            var (prompt, correctAnswer, wrongPool) = direction == QuizDirection.UzToRu
                ? (w.WordUz, w.WordRu, words.Where(x => x.Id != w.Id).Select(x => x.WordRu).ToList())
                : (w.WordRu, w.WordUz, words.Where(x => x.Id != w.Id).Select(x => x.WordUz).ToList());

            var wrongAnswers = wrongPool
                .OrderBy(_ => Random.Shared.Next())
                .Take(3)
                .ToList();

            var allOptions = wrongAnswers.Append(correctAnswer)
                .OrderBy(_ => Random.Shared.Next())
                .Select((text, idx) => new QuizOptionDto(idx + 1, text))
                .ToList();

            var directionLabel = direction == QuizDirection.UzToRu ? "ruscha" : "o'zbekcha";
            var questionText = $"\"{prompt}\" so'zining {directionLabel} tarjimasi?";
            var correctAnswerHash = ComputeHash(correctAnswer);

            return new QuizQuestionDto(
                Guid.NewGuid(),
                i + 1,
                count,
                questionText,
                allOptions,
                correctAnswerHash);
        }).ToList();

        return new QuizStartResponseDto(
            Guid.NewGuid(),
            userId,
            direction,
            questions);
    }

    private string ComputeHash(string answer)
    {
        var input = _settings.SharedSalt + answer;
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
