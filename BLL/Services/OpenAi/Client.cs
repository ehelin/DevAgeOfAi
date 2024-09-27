using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLL.Services.OpenAi
{
    public class Client
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiUrl = "https://api.openai.com/v1/completions";

        public Client(string apiKey)
        {
            _httpClient = new HttpClient();
            _apiKey = apiKey;
        }

        public async Task<string> GetCompletionAsync(string prompt)
        {
            var requestBody = new
            {
                model = "text-davinci-003", // or another available model
                prompt,
                max_tokens = 100,
                temperature = 0.7
            };

            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var requestContent = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PostAsync(ApiUrl, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get completion from OpenAI: {response.StatusCode}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseString);

            // Extracting the text from the response JSON.
            return responseJson.GetProperty("choices")[0].GetProperty("text").GetString();
        }
    }
}
