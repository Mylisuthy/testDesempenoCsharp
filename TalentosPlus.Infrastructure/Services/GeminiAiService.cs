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

        // Enhanced Prompt for better context and language support
        var prompt = $@"
Context Data (JSON format): 
{contextData}

User Question: 
{question}

Instructions:
1. Answer the user's question based strictly on the provided Context Data.
2. If the user asks in Spanish, answer in Spanish. If they ask in English, answer in English.
3. Be professional, concise, and helpful.
4. If the answer is not found in the data, state clearly that you don't have that information.
5. Do not hallucinate or make up data.
";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try 
        {
            var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                
                if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var text = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                    return text ?? "No answer generated.";
                }
            }
            else 
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // Structured logging in a real app, Console for now
                Console.WriteLine($"Gemini API Error: {response.StatusCode} - {errorContent}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return "Error: API Key is invalid or expired. Please check configuration.";
                
                return $"Error connecting to AI Assistant ({response.StatusCode}). Please try again later.";
            }
        }
        catch (Exception ex)
        {
             Console.WriteLine($"Gemini Exception: {ex.Message}");
             return "An error occurred while communicating with the AI service.";
        }

        return "No valid response from AI.";
    }
}
