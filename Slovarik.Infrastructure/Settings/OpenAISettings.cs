namespace Slovarik.Infrastructure.Settings;

public class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
}
