using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class AiModelApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl = "http://127.0.0.1:8000/generate";

    public AiModelApiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string?> GenerateResponseAsync(string prompt)
    {
        var requestPayload = new { prompt };

        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(requestPayload),
                Encoding.UTF8,
                "application/json"
            );

            using var response = await _httpClient.PostAsync(_apiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("response", out var resultElement))
            {
                return resultElement.GetString();
            }

            return $"Error: Unexpected response format: {responseBody}";
        }
        catch (HttpRequestException ex)
        {
            return $"HTTP error: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Unexpected error: {ex.Message}";
        }
    }
}
