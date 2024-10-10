namespace Tests
{
    public class EnvironmentManagerTests
    {
        [Fact]
        public void GetOpenAiKey_Test()
        {
            var result = EnvironmentManager.GetVariable("OpenAiKey");

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetMsAiKey_Test()
        {
            var result = EnvironmentManager.GetVariable("MsAiKey");

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}