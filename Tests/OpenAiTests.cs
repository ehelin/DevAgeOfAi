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
        public void GetSuggestion_Test()
        {
            var result = service.GetHabitToTrackSuggestion();

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}