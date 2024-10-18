using BLL.Ai.Services;
using Shared.Interfaces;
using OpenAiClient = BLL.Ai.Clients.OpenAi;

namespace Tests
{
    public class OpenAiTests
    {
        private readonly IThirdPartyAiService service;
        private readonly OpenAiClient.Client client;

        public OpenAiTests()
        {
            var aiKey = EnvironmentManager.GetVariable("OpenAiKey");

            IClient client = new OpenAiClient.Client(aiKey);
            IEnumerable<IClient> clients = new List<IClient> { client };

            service = new OpenAiService(clients);
        }

        [Fact]
        public async Task GetSuggestion_Test()
        {
            var result = await service.GetHabitToTrackSuggestion();

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}