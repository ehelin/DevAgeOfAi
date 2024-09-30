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
    }
}