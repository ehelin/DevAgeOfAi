using Shared.interfaces;
using BLL.Services.OpenAi;

namespace Tests
{
    public class OpenAiTests
    {
        private readonly IThirdPartyAiService service;

        public OpenAiTests()
        {
            service = new Service();
        }

        [Fact]
        public void GetSuggestion_Test()
        {
            var prompt = "Tell me something";

            var result = service.GetSuggestion(prompt);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}