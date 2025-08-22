using BLL.Ai.Clients.PythonAi;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class AiModelHttpClientTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private Process _pythonServerProcess;
        private AiModelHttpClient _client;
        private readonly HttpClient _httpClient;

        public AiModelHttpClientTests(ITestOutputHelper output)
        {
            _output = output;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _client = new AiModelHttpClient(_httpClient);
            
            StartPythonServer();
            WaitForServerToStart();
        }

        private void StartPythonServer()
        {
            try
            {
                var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\"));
                var pythonScriptPath = Path.Combine(projectRoot, @"Ai\AiModelRunner\ai_model_server.py");
                
                _output.WriteLine($"Starting Python server from: {pythonScriptPath}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = pythonScriptPath,
                    WorkingDirectory = Path.GetDirectoryName(pythonScriptPath),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                _pythonServerProcess = Process.Start(startInfo);
                
                // Start async readers for output
                Task.Run(() => ReadProcessOutput(_pythonServerProcess.StandardOutput));
                Task.Run(() => ReadProcessOutput(_pythonServerProcess.StandardError));
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Failed to start Python server: {ex.Message}");
                throw;
            }
        }

        private void ReadProcessOutput(StreamReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                _output.WriteLine($"Python Server: {line}");
            }
        }

        private void WaitForServerToStart()
        {
            _output.WriteLine("Waiting for Python server to start...");
            
            for (int i = 0; i < 30; i++)
            {
                Thread.Sleep(1000);
                
                if (_client.HealthCheckAsync().GetAwaiter().GetResult())
                {
                    _output.WriteLine("Python server is ready!");
                    return;
                }
            }
            
            throw new TimeoutException("Python server failed to start within 30 seconds");
        }

        [Fact]
        public async Task HealthCheck_ShouldReturnTrue_WhenServerIsRunning()
        {
            // Act
            var result = await _client.HealthCheckAsync();

            // Assert
            Assert.True(result, "Health check should return true when server is running");
        }

        [Fact]
        public async Task GetCompletionAsync_ShouldReturnResponse_ForHabitSuggestion()
        {
            // Arrange
            string prompt = "Suggest a habit for improving productivity";

            // Act
            var response = await _client.GetCompletionAsync(prompt);

            // Assert
            Assert.NotNull(response);
            Assert.NotEmpty(response);
            Assert.NotEqual("No response received", response);
            _output.WriteLine($"AI Response: {response}");
        }

        [Fact]
        public void GetCompletion_ShouldReturnResponse_ForHabitSuggestion()
        {
            // Arrange
            string prompt = "Suggest a habit for better sleep";

            // Act
            var response = _client.GetCompletion(prompt);

            // Assert
            Assert.NotNull(response);
            Assert.NotEmpty(response);
            Assert.NotEqual("No response received", response);
            _output.WriteLine($"AI Response: {response}");
        }

        [Fact]
        public async Task GetCompletionAsync_ShouldHandleMultipleRequests()
        {
            // Arrange
            var prompts = new[]
            {
                "Suggest a habit for exercise",
                "Suggest a habit for reading",
                "Suggest a habit for meditation"
            };

            // Act & Assert
            foreach (var prompt in prompts)
            {
                var response = await _client.GetCompletionAsync(prompt);
                
                Assert.NotNull(response);
                Assert.NotEmpty(response);
                _output.WriteLine($"Prompt: {prompt}");
                _output.WriteLine($"Response: {response}");
                _output.WriteLine("---");
            }
        }

        [Fact]
        public async Task GetHabitToTrackSuggestion_ShouldReturnHabitSuggestion()
        {
            // Act
            var response = await _client.GetHabitToTrackSuggestion();

            // Assert
            Assert.NotNull(response);
            Assert.NotEmpty(response);
            Assert.NotEqual("No response received", response);
            _output.WriteLine($"Habit Suggestion: {response}");
        }

        [Fact]
        public async Task GetHabitsFromPrompt_ShouldReturnResponse()
        {
            // Arrange
            string prompt = "Give me 3 habits for improving health";

            // Act
            var response = await _client.GetHabitsFromPrompt(prompt);

            // Assert
            Assert.NotNull(response);
            Assert.NotEmpty(response);
            Assert.NotEqual("No response received", response);
            _output.WriteLine($"Habits from prompt: {response}");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            
            if (_pythonServerProcess != null && !_pythonServerProcess.HasExited)
            {
                try
                {
                    _pythonServerProcess.Kill();
                    _pythonServerProcess.WaitForExit(5000);
                    _pythonServerProcess.Dispose();
                    _output.WriteLine("Python server stopped");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Error stopping Python server: {ex.Message}");
                }
            }
        }
    }
}