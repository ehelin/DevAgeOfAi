using Shared.interfaces;
using BLL.Services.MicrosoftAi;

namespace Tests
{
    public class MsAiTests
    {
        private readonly IThirdPartyAiService service;
        private readonly Client client;

        public MsAiTests()
        {
            var aiKey = EnvironmentManager.GetVariable("MsAiKey");

            client = new Client(aiKey);
            service = new BLL.Services.Service(client);
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