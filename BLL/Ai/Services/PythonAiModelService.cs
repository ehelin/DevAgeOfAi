using BLL.Ai.Clients.PythonAi;
using Shared.Interfaces;

namespace BLL.Ai.Services
{
    public class PythonAiModelService : IThirdPartyAiService, IDisposable
    {
        private readonly AiModelHttpClient _client;
        private readonly HttpClient _httpClient;

        public PythonAiModelService(string baseUrl = "http://localhost:5000")
        {
            _httpClient = new HttpClient 
            { 
                Timeout = TimeSpan.FromSeconds(30)
            };
            _client = new AiModelHttpClient(_httpClient, baseUrl);
        }

        public PythonAiModelService(HttpClient httpClient, string baseUrl = "http://localhost:5000")
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _client = new AiModelHttpClient(_httpClient, baseUrl);
        }

        public async Task<string> GetHabitToTrackSuggestion()
        {
            try
            {
                // First check if server is running
                if (!await _client.HealthCheckAsync())
                {
                    return "Python AI model server is not running. Please start ai_model_server.py first.";
                }

                return await _client.GetHabitToTrackSuggestion();
            }
            catch (HttpRequestException ex)
            {
                return $"Error connecting to Python AI server: {ex.Message}. Make sure ai_model_server.py is running.";
            }
            catch (Exception ex)
            {
                return $"Unexpected error: {ex.Message}";
            }
        }

        public async Task<string> GetHabitsFromPrompt(string prompt)
        {
            try
            {
                // First check if server is running
                if (!await _client.HealthCheckAsync())
                {
                    return "Python AI model server is not running. Please start ai_model_server.py first.";
                }

                return await _client.GetHabitsFromPrompt(prompt);
            }
            catch (HttpRequestException ex)
            {
                return $"Error connecting to Python AI server: {ex.Message}. Make sure ai_model_server.py is running.";
            }
            catch (Exception ex)
            {
                return $"Unexpected error: {ex.Message}";
            }
        }

        public string GetCompletion(string prompt)
        {
            try
            {
                // First check if server is running
                if (!_client.HealthCheckAsync().GetAwaiter().GetResult())
                {
                    return "Python AI model server is not running. Please start ai_model_server.py first.";
                }

                return _client.GetCompletion(prompt);
            }
            catch (HttpRequestException ex)
            {
                return $"Error connecting to Python AI server: {ex.Message}. Make sure ai_model_server.py is running.";
            }
            catch (Exception ex)
            {
                return $"Unexpected error: {ex.Message}";
            }
        }

        public async Task<string> GetCompletionAsync(string prompt)
        {
            try
            {
                // First check if server is running
                if (!await _client.HealthCheckAsync())
                {
                    return "Python AI model server is not running. Please start ai_model_server.py first.";
                }

                return await _client.GetCompletionAsync(prompt);
            }
            catch (HttpRequestException ex)
            {
                return $"Error connecting to Python AI server: {ex.Message}. Make sure ai_model_server.py is running.";
            }
            catch (Exception ex)
            {
                return $"Unexpected error: {ex.Message}";
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}