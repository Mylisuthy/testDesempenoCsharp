using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TalentosPlus.Domain.Interfaces;

namespace TalentosPlus.Infrastructure.Services;

public class GeminiAiService : IAiService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public GeminiAiService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<string> AskQuestionAsync(string question, string contextData)
    {
        var apiKey = _configuration["AiSettings:GeminiApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            return "AI Service is not configured.";
        }

        var prompt = $"Context Data (JSON format of database stats): {contextData}\n\nUser Question: {question}\n\nAnswer the user's question based strictly on the provided Context Data. Do not hallucinate.";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}", content);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            // Parse Gemini response structure to get the text
            if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var text = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                return text ?? "No answer generated.";
            }
        }

        return "Error contacting AI service.";
    }
}
