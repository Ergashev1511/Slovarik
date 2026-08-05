using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Slovarik.Application.DTOs;
using Slovarik.Application.Interfaces;
using Slovarik.Infrastructure.Settings;
using UglyToad.PdfPig;

namespace Slovarik.Infrastructure.Services;

public class OpenAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAISettings _settings;
    private readonly ILogger<OpenAIService> _logger;

    private const string SystemPrompt =
        "You are a Uzbek-Russian dictionary OCR parser.\n" +
        "Read the image/text and output word pairs as JSON.\n" +
        "Format: {\"words\":[{\"wordUz\":\"kitob\",\"wordRu\":\"книга\",\"categoryName\":\"Boshqa so'zlar\"}]}\n" +
        "Rules: wordUz=Uzbek, wordRu=Russian(Cyrillic), correct spelling, no extra text, no markdown, ONLY JSON.\n" +
        "Assign each pair to a category from the list. If unsure, use \"Boshqa so'zlar\".\n" +
        "Skip non-word lines (page numbers, headers).";

    public OpenAIService(
        HttpClient httpClient,
        IOptions<OpenAISettings> settings,
        ILogger<OpenAIService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
    }

    public async Task<List<WordPairDto>> ExtractWordsFromImageAsync(
        byte[] imageBytes,
        string contentType,
        IReadOnlyList<string> categoryNames,
        CancellationToken ct = default)
    {
        var base64 = Convert.ToBase64String(imageBytes);
        var dataUrl = $"data:{contentType};base64,{base64}";

        var userPrompt = BuildUserPrompt(categoryNames);

        var requestBody = new
        {
            model = _settings.Model,
            max_tokens = 16384,
            reasoning = new { enabled = false },
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = userPrompt },
                        new { type = "image_url", image_url = new { url = dataUrl } }
                    }
                }
            },
        };

        return await SendChatRequestAsync(requestBody, ct);
    }

    public async Task<List<WordPairDto>> ExtractWordsFromPdfAsync(byte[] pdfBytes, IReadOnlyList<string> categoryNames, CancellationToken ct = default)
    {
        var text = ExtractTextFromPdf(pdfBytes);

        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("PDF dan matn topilmadi.");
            return new List<WordPairDto>();
        }

        var userPrompt = BuildUserPrompt(categoryNames);

        var requestBody = new
        {
            model = _settings.Model,
            max_tokens = 16384,
            reasoning = new { enabled = false },
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user", content = $"{userPrompt}\n\n{text}" }
            },
        };

        return await SendChatRequestAsync(requestBody, ct);
    }

    private async Task<List<WordPairDto>> SendChatRequestAsync(object requestBody, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/chat/completions", content, ct);
        var responseJson = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenAI API xatosi: {StatusCode} - {Body}", response.StatusCode, responseJson);
            return new List<WordPairDto>();
        }

        return ParseResponse(responseJson);
    }

    private List<WordPairDto> ParseResponse(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var messageContent = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(messageContent))
                return new List<WordPairDto>();

            // Model javobini markdown code fencedan tozalash
            messageContent = StripCodeFences(messageContent);

            // Reasoning taglardan va boshqa matndan JSON'ni ajratib olish
            messageContent = ExtractJsonFromContent(messageContent);

            using var contentDoc = JsonDocument.Parse(messageContent);
            var wordsArray = contentDoc.RootElement.GetProperty("words");

            var result = new List<WordPairDto>();
            foreach (var item in wordsArray.EnumerateArray())
            {
                var wordUz = item.GetProperty("wordUz").GetString() ?? "";
                var wordRu = item.GetProperty("wordRu").GetString() ?? "";

                if (!string.IsNullOrWhiteSpace(wordUz) && !string.IsNullOrWhiteSpace(wordRu))
                {
                    var categoryName = item.TryGetProperty("categoryName", out var catEl)
                        ? catEl.GetString()
                        : null;
                    result.Add(new WordPairDto(wordUz.Trim(), wordRu.Trim(), categoryName?.Trim()));
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI javobini parse qilishda xatolik.");
            return new List<WordPairDto>();
        }
    }

    private static string StripCodeFences(string text)
    {
        var trimmed = text.Trim();

        // ```json ... ``` yoki ``` ... ``` formatni tozalash
        if (trimmed.StartsWith("```"))
        {
            // Birinchi qatorni olib tashlamiz (```json yoki ```)
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline >= 0)
                trimmed = trimmed[(firstNewline + 1)..];

            // Oxirgi ``` ni olib tashlamiz
            var lastFence = trimmed.LastIndexOf("```");
            if (lastFence >= 0)
                trimmed = trimmed[..lastFence];

            trimmed = trimmed.Trim();
        }

        return trimmed;
    }

    private static string ExtractJsonFromContent(string content)
    {
        // <think>...</think> yoki <reasoning>...</reasoning> bloklarni olib tashlash
        var thinkStart = content.IndexOf("<think>");
        if (thinkStart >= 0)
        {
            var thinkEnd = content.IndexOf("</think>", thinkStart);
            if (thinkEnd >= 0)
                content = content.Remove(thinkStart, thinkEnd + "</think>".Length - thinkStart);
        }

        var reasoningStart = content.IndexOf("<reasoning>");
        if (reasoningStart >= 0)
        {
            var reasoningEnd = content.IndexOf("</reasoning>", reasoningStart);
            if (reasoningEnd >= 0)
                content = content.Remove(reasoningStart, reasoningEnd + "</reasoning>".Length - reasoningStart);
        }

        content = content.Trim();

        // Agar to'g'ridan JSON bo'lsa, qaytaramiz
        if (content.StartsWith("{"))
            return content;

        // Aks holda, birinchi { dan oxirgi } gacha bo'lgan qismini olamiz
        var firstBrace = content.IndexOf('{');
        var lastBrace = content.LastIndexOf('}');

        if (firstBrace >= 0 && lastBrace > firstBrace)
            return content.Substring(firstBrace, lastBrace - firstBrace + 1);

        return content;
    }

    private static string BuildUserPrompt(IReadOnlyList<string> categoryNames)
    {
        var categories = categoryNames.Count > 0
            ? string.Join(", ", categoryNames.Select(c => $"\"{c}\""))
            : "\"Boshqa so'zlar\"";

        return $"Categories: {categories}. Output ONLY JSON, no explanation.";
    }

    private static string ExtractTextFromPdf(byte[] pdfBytes)
    {
        var sb = new StringBuilder();

        using var stream = new MemoryStream(pdfBytes);
        using var document = PdfDocument.Open(stream);

        foreach (var page in document.GetPages())
        {
            sb.AppendLine(page.Text);
        }

        return sb.ToString();
    }
}
