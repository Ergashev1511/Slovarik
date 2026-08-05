using System.Text.Json.Serialization;

namespace Slovarik.Application.DTOs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuizDirection
{
    UzToRu,
    RuToUz
}

public class QuizStartRequestDto
{
    public Guid UserId { get; set; }
    public int QuestionCount { get; set; }
    public QuizDirection Direction { get; set; } = QuizDirection.UzToRu;
    public bool SelectFromEnd { get; set; } = false;
    public Guid? CategoryId { get; set; }
}

public record QuizStartResponseDto(
    Guid QuizId,
    Guid UserId,
    QuizDirection Direction,
    List<QuizQuestionDto> Questions);

public record QuizQuestionDto(
    Guid QuestionId,
    int QuestionNumber,
    int TotalQuestions,
    string Question,
    List<QuizOptionDto> Options,
    string CorrectAnswerHash);

public record QuizOptionDto(int Id, string Text);
