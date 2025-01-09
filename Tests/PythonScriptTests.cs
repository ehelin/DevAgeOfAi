using BLL.Ai.Services;
using Shared;
using Shared.Interfaces;

namespace Tests
{
    public class PythonScriptTests
    {
        private const string ScriptPath = "C:\\temp\\New folder\\Ai\\AiModelRunner\\PythonApplication1.py";
        private readonly IPythonScriptService _scriptService;

        public PythonScriptTests()
        {
            // Initialize the Python script service
            _scriptService = new PythonScriptService(ScriptPath);
            _scriptService.StartAsync().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task TestPythonScriptResponseHabits_ToTrack()
        {
            var input = Constants.HABIT_TO_TRACK_PROMPT;
            // Send input to the Python script and get the response
            var response = await _scriptService.SendInputAsync(input);

            // Validate the response
            Assert.NotNull(response);
            Assert.True(response.Length > 0);
        }

        [Theory]
        [InlineData("Can you tell me about yourself?")]
        [InlineData("Can you tell me one good joke?")]
        [InlineData("Do you have an opinion on jokes?")]
        [InlineData("Can you tell me something smart?")]
        public async Task TestPythonScriptResponse(string input)
        {
            // Send input to the Python script and get the response
            var response = await _scriptService.SendInputAsync(input);

            // Validate the response
            Assert.NotNull(response);
            Assert.True(response.Length > 0);
        }

        public void Dispose()
        {
            // Ensure the service is properly disposed after tests
            _scriptService.Stop();
        }
    }
}
