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
        "You are an expert Uzbek-Russian dictionary parser.\n" +
        "Your task: read the provided text (it may be raw OCR output, a dictionary page, or a word list)\n" +
        "and extract EVERY Uzbek-Russian word pair you can find.\n\n" +
        "Common dictionary formats you will encounter:\n" +
        "  - \"kitob - книга\"  (separated by a dash)\n" +
        "  - \"kitob\tкнига\"   (separated by a tab or multiple spaces)\n" +
        "  - \"kitob книга\"    (separated by a single space, Uzbek first)\n" +
        "  - One pair per line, or multiple pairs on the same line.\n" +
        "  - Sometimes the Russian side may contain multiple translations: \"kitob - книга, том\" —\n" +
        "    in that case keep only the primary/first translation as wordRu.\n\n" +
        "Rules:\n" +
        "  1. wordUz = the Uzbek word (Latin or Cyrillic script).\n" +
        "  2. wordRu = the Russian translation (Cyrillic).\n" +
        "  3. Skip page numbers, headers, footers, and any line that is not a word pair.\n" +
        "  4. Trim whitespace. Remove trailing punctuation from both fields.\n" +
        "  5. If a word contains a parenthetical note like \"olma (daraxt)\", keep only \"olma\".\n" +
        "  6. Do NOT invent pairs — only extract what is actually in the text.\n\n" +
        "Output format — return ONLY a valid JSON object, no markdown, no code fences, no explanation:\n" +
        "{\"words\":[{\"wordUz\":\"kitob\",\"wordRu\":\"книга\"},{\"wordUz\":\"olma\",\"wordRu\":\"яблоко\"}]}\n\n" +
        "If no word pairs are found, return: {\"words\":[]}";

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
        CancellationToken ct = default)
    {
        var base64 = Convert.ToBase64String(imageBytes);
        var dataUrl = $"data:{contentType};base64,{base64}";

        var requestBody = new
        {
            model = _settings.Model,
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = "Extract all Uzbek-Russian word pairs from this image." },
                        new { type = "image_url", image_url = new { url = dataUrl } }
                    }
                }
            },
        };

        return await SendChatRequestAsync(requestBody, ct);
    }

    public async Task<List<WordPairDto>> ExtractWordsFromPdfAsync(byte[] pdfBytes, CancellationToken ct = default)
    {
        var text = ExtractTextFromPdf(pdfBytes);

        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("PDF dan matn topilmadi.");
            return new List<WordPairDto>();
        }

        var requestBody = new
        {
            model = _settings.Model,
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user", content = $"Extract all Uzbek-Russian word pairs from the following text:\n\n{text}" }
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

            using var contentDoc = JsonDocument.Parse(messageContent);
            var wordsArray = contentDoc.RootElement.GetProperty("words");

            var result = new List<WordPairDto>();
            foreach (var item in wordsArray.EnumerateArray())
            {
                var wordUz = item.GetProperty("wordUz").GetString() ?? "";
                var wordRu = item.GetProperty("wordRu").GetString() ?? "";

                if (!string.IsNullOrWhiteSpace(wordUz) && !string.IsNullOrWhiteSpace(wordRu))
                {
                    result.Add(new WordPairDto(wordUz.Trim(), wordRu.Trim()));
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
