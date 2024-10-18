using Shared.Interfaces;

namespace BLL.Ai.Clients
{
    public class ClientFactory : IClientFactory
    {
        private readonly BLL.Ai.Clients.OpenAi.Client _openAiClient;
        private readonly BLL.Ai.Clients.MicrosoftAi.Client _microsoftAiClient;

        public ClientFactory(OpenAi.Client openAiClient, MicrosoftAi.Client microsoftAiClient)
        {
            _openAiClient = openAiClient;
            _microsoftAiClient = microsoftAiClient;
        }

        public IClient GetClient(string clientType)
        {
            return clientType switch
            {
                "OpenAi" => _openAiClient,
                "MicrosoftAi" => _microsoftAiClient,
                _ => throw new ArgumentException("Invalid client type"),
            };
        }
    }

}
