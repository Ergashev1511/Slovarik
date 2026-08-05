using Slovarik.Application.DTOs;
using Slovarik.Application.Interfaces;

namespace Slovarik.Api.Endpoints;

public static class QuizEndpoints
{
    public static void MapQuizEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/quiz");

        group.MapPost("/start", async (QuizStartRequestDto request, IQuizService quizService, CancellationToken ct) =>
        {
            try
            {
                var response = await quizService.StartQuizAsync(request.UserId, request.QuestionCount, request.Direction, request.SelectFromEnd, request.CategoryId, ct);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new ErrorResponseDto(ex.Message));
            }
        })
        .WithName("StartQuiz")
        .WithOpenApi();
    }
}
