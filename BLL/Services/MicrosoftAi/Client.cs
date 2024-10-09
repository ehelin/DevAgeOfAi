using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLL.Services.MicrosoftAi
{
    public class Client
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiUrl = "";

        public Client(string apiKey)
        {
            _httpClient = new HttpClient();
            _apiKey = apiKey;
        }

        public async Task<string> GetCompletionAsync(string prompt)
        {
            throw new NotImplementedException();
        }
    }
}
