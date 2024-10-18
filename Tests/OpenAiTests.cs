using Shared.Interfaces;
using BLL.Ai.Clients.OpenAi;

namespace Tests
{
    public class OpenAiTests
    {
        private readonly IThirdPartyAiService service;
        private readonly Client client;

        public OpenAiTests()
        {
            var aiKey = EnvironmentManager.GetVariable("OpenAiKey");

            client = new Client(aiKey);
            service = new BLL.Ai.Services.OpenAiService(client);
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