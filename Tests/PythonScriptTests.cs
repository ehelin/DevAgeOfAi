using BLL.Ai.Services;
using Shared;
using Shared.Interfaces;
using System.Text.Json;

namespace Tests
{
    public class PythonScriptTests : IDisposable
    {
        // Updated path to use the new test wrapper
        private const string ScriptPath = "C:\\temp\\faease\\Ai\\AiModelRunner\\ai_model_test_wrapper.py";
        private readonly IPythonScriptService _scriptService;
        private bool _disposed = false;

        public PythonScriptTests()
        {
            // Initialize the Python script service with the test wrapper
            _scriptService = new PythonScriptService(ScriptPath);
            _scriptService.StartAsync().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task TestPythonScriptResponseHabits_ToTrack_Single()
        {
            var input = Constants.HABIT_TO_TRACK_PROMPT;

            // Send input to the Python script and get the response
            var response = await _scriptService.SendInputAsync(input);

            // Validate the response
            Assert.NotNull(response);
            Assert.True(response.Length > 0);
        }

        [Fact]
        public async Task TestPythonScriptResponseHabits_ToTrack()
        {
            var responses = new List<string>();
            for (var i = 0; i < 50; i++)
            {
                var input = Constants.HABIT_TO_TRACK_PROMPT;

                // Send input to the Python script and get the response
                var response = await _scriptService.SendInputAsync(input);

                // Validate the response
                Assert.NotNull(response);
                Assert.True(response.Length > 0);

                responses.Add(response);
            }

            responses.Sort();

            var done = 1;
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
            
            // Check if response is JSON (error case)
            if (response.StartsWith("{"))
            {
                var jsonResponse = JsonSerializer.Deserialize<JsonDocument>(response);
                Assert.True(jsonResponse.RootElement.TryGetProperty("status", out var status));
                Assert.Equal("success", status.GetString());
            }
        }

        [Fact]
        public async Task TestPythonScriptWithJsonInput()
        {
            // Test sending JSON formatted input with custom parameters
            var jsonInput = JsonSerializer.Serialize(new
            {
                prompt = "Please suggest a habit that can be tracked",
                @params = new
                {
                    temperature = 0.7,
                    max_new_tokens = 15
                }
            });

            var response = await _scriptService.SendInputAsync(jsonInput);

            // Validate the response
            Assert.NotNull(response);
            Assert.True(response.Length > 0);

            // Parse JSON response if applicable
            if (response.StartsWith("{"))
            {
                var jsonResponse = JsonSerializer.Deserialize<JsonDocument>(response);
                Assert.True(jsonResponse.RootElement.TryGetProperty("status", out var status));
                Assert.Equal("success", status.GetString());
                Assert.True(jsonResponse.RootElement.TryGetProperty("response", out var habitResponse));
                Assert.NotNull(habitResponse.GetString());
            }
        }

        [Fact]
        public async Task TestMultipleSequentialRequests()
        {
            // Test that the service can handle multiple sequential requests
            var prompts = new[]
            {
                "Suggest a morning habit",
                "Suggest an evening habit",
                "Suggest a weekend habit"
            };

            foreach (var prompt in prompts)
            {
                var response = await _scriptService.SendInputAsync(prompt);
                
                Assert.NotNull(response);
                Assert.True(response.Length > 0);
                Assert.DoesNotContain("error", response, StringComparison.OrdinalIgnoreCase);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Ensure the service is properly disposed after tests
                    _scriptService?.Stop();
                }
                _disposed = true;
            }
        }
    }
}
