using Newtonsoft.Json;
using Shared.Interfaces;
using System.Text;

namespace BLL.Ai.Clients.MicrosoftAi
{
    public class Client : IClient
    {
        private readonly string _endpointUrl = "https://{your-resource-name}.openai.azure.com";
        private readonly string _deploymentId = "your deployment id here";
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public Client(string deploymentId, string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetCompletionAsync(string input)
        {
            var requestUri = $"{_endpointUrl}/openai/deployments/{_deploymentId}/chat/completions?api-version=2024-06-01";
            var requestBody = new
            {
                messages = new[]
                {
                new { role = "system", content = "You are an assistant." },
                new { role = "user", content = input }
            },
                temperature = 0.7,
                max_tokens = 100
            };

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(responseData);

            return result.choices[0].message.content.ToString();
        }
    }
}
