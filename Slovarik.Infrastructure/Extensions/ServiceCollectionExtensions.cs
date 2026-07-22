using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Slovarik.Application.Interfaces;
using Slovarik.Application.Services;
using Slovarik.Application.Settings;
using Slovarik.Domain.Interfaces;
using Slovarik.Infrastructure.Data;
using Slovarik.Infrastructure.Repositories;
using Slovarik.Infrastructure.Services;
using Slovarik.Infrastructure.Settings;
using Slovarik.Infrastructure.Telegram;
using Telegram.Bot;

namespace Slovarik.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Database
        services.AddDbContext<SlovarikDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWordRepository, WordRepository>();

        // Application services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWordService, WordService>();
        services.AddScoped<IQuizService, QuizService>();

        // AI Service
        services.Configure<OpenAISettings>(config.GetSection("OpenAI"));
        services.AddHttpClient<IAIService, OpenAIService>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        // Quiz settings
        services.Configure<QuizSettings>(config.GetSection("Quiz"));

        // Telegram bot
        services.Configure<TelegramBotSettings>(config.GetSection("Telegram"));
        services.AddSingleton<ITelegramBotClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<TelegramBotSettings>>().Value;
            return new TelegramBotClient(settings.BotToken);
        });
        services.AddHostedService<TelegramBotService>();

        return services;
    }
}
