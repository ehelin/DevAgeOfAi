using BLL.Ai.Clients.PythonAi;
using System.Diagnostics;

namespace Tests
{
    public class TestAiModelClient
    {
        public static async Task RunManualTest()
        {
            Console.WriteLine("=== Python AI Model Client Test ===\n");
            
            // Check if Python server is running
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var client = new AiModelHttpClient(httpClient);
            
            Console.WriteLine("1. Checking server health...");
            var isHealthy = await client.HealthCheckAsync();
            
            if (!isHealthy)
            {
                Console.WriteLine("❌ Server is not running!");
                Console.WriteLine("\nPlease start the Python server first:");
                Console.WriteLine("  cd Ai\\AiModelRunner");
                Console.WriteLine("  python ai_model_server.py");
                return;
            }
            
            Console.WriteLine("✅ Server is healthy!\n");
            
            // Test habit suggestions using the interface methods
            Console.WriteLine("2. Testing GetHabitToTrackSuggestion method...\n");
            
            try
            {
                var habitSuggestion = await client.GetHabitToTrackSuggestion();
                Console.WriteLine($"Habit Suggestion: {habitSuggestion}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            Console.WriteLine("\n3. Testing GetHabitsFromPrompt method...\n");
            
            var testPrompts = new[]
            {
                "Suggest a habit for better morning routine",
                "Suggest a habit for improving focus",
                "Suggest a habit for stress management",
                "Give me 3 habits for learning new skills"
            };
            
            foreach (var prompt in testPrompts)
            {
                Console.WriteLine($"Prompt: {prompt}");
                
                try
                {
                    var response = await client.GetHabitsFromPrompt(prompt);
                    Console.WriteLine($"Response: {response}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                
                Console.WriteLine();
            }
            
            // Interactive test
            Console.WriteLine("\n4. Interactive test - Enter your own prompts (type 'exit' to quit):\n");
            
            while (true)
            {
                Console.Write("Your prompt: ");
                var userPrompt = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(userPrompt) || userPrompt.ToLower() == "exit")
                    break;
                
                try
                {
                    var response = await client.GetCompletionAsync(userPrompt);
                    Console.WriteLine($"AI Response: {response}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}\n");
                }
            }
            
            Console.WriteLine("\nTest completed!");
        }
        
        // Entry point for standalone testing
        public static async Task Main(string[] args)
        {
            await RunManualTest();
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}