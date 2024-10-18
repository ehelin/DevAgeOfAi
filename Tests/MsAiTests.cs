using Shared.Interfaces;
using MsAiClient = BLL.Ai.Clients.MicrosoftAi;
using MsAiService = BLL.Ai.Services;

namespace Tests
{
    public class MsAiTests
    {
        private readonly IThirdPartyAiService service;
        private readonly MsAiClient.Client client;

        public MsAiTests()
        {
            var aiKey = EnvironmentManager.GetVariable("MsAiKey"); 
            var msAiDeploymentId = EnvironmentManager.GetVariable("MsAiDeploymentId");

            client = new MsAiClient.Client(aiKey, msAiDeploymentId);
            service = new MsAiService.MicrosoftAiService(client);
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