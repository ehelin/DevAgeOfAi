using System.Net.Http.Json;
using System.Text.Json;
using Shared;
using Shared.Interfaces;

namespace BLL.Ai.Clients.PythonAi
{
    public class AiModelHttpClient : IThirdPartyAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public AiModelHttpClient(HttpClient httpClient, string baseUrl = "http://localhost:5000")
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = baseUrl;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<string> GetHabitToTrackSuggestion()
        {
            // Use the constant prompt for habit suggestions
            return await GetCompletionAsync(Constants.HABIT_TO_TRACK_PROMPT);
        }

        public async Task<string> GetHabitsFromPrompt(string prompt)
        {
            return await GetCompletionAsync(prompt);
        }

        public async Task<string> GetCompletionAsync(string prompt)
        {
            try
            {
                var request = new
                {
                    message = prompt
                };

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/chat", request);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ChatResponse>(responseContent, _jsonOptions);

                return result?.Response ?? "No response received";
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Error communicating with Python AI model server: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new InvalidOperationException("Request to Python AI model server timed out", ex);
            }
        }

        public string GetCompletion(string prompt)
        {
            return GetCompletionAsync(prompt).GetAwaiter().GetResult();
        }

        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/health");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var healthResponse = JsonSerializer.Deserialize<HealthResponse>(content, _jsonOptions);
                    return healthResponse?.Status == "ready";
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private class ChatResponse
        {
            public string? Response { get; set; }
            public string? Model { get; set; }
            public string? Status { get; set; }
        }

        private class HealthResponse
        {
            public string? Status { get; set; }
            public string? Model { get; set; }
        }
    }
}