using System.Text;
using System.Text.Json;

namespace TaskManager.Services
{
    public class ClaudeService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ClaudeService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["ClaudeApiKey"];
        }

        public async Task<string> SuggestPriorityAsync(string taskTitle)
        {
            var requestBody = new
            {
                model = "claude-haiku-4-5-20251001",
                max_tokens = 100,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = $"Based on this task title, suggest a priority: Low, Medium, or High. Reply with one word only.\nTask: {taskTitle}"
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
            var responseString = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Claude response: {responseString}");
            //var responseString = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Claude response: {responseString}");
            var doc = JsonDocument.Parse(responseString);
            if (doc.RootElement.TryGetProperty("content", out var contentArray))
            {
                return contentArray[0].GetProperty("text").GetString()?.Trim() ?? "Medium";
            }
            return "Medium";
        }
    }
}