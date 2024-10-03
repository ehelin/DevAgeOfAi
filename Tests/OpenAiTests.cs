using Shared.interfaces;
using BLL.Services.OpenAi;

namespace Tests
{
    public class OpenAiTests
    {
        private readonly IThirdPartyAiService service;
        private readonly Client client;

        public OpenAiTests()
        {
            var openAiKey = EnvironmentManager.GetVariable("OpenAiKey");

            client = new Client(openAiKey);
            service = new Service(client);
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