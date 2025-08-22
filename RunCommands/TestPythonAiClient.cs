using BLL.Ai.Services;
using System;
using System.Threading.Tasks;

namespace RunCommands
{
    public class TestPythonAiClient
    {
        public static async Task RunTest()
        {
            Console.WriteLine("===================================");
            Console.WriteLine("Testing Python AI Model HTTP Client");
            Console.WriteLine("===================================\n");

            // Create the Python AI Model service
            var aiService = new PythonAiModelService();

            // Test 1: Get a habit suggestion
            Console.WriteLine("Test 1: Getting habit suggestion...");
            var habitSuggestion = await aiService.GetHabitToTrackSuggestion();
            Console.WriteLine($"Result: {habitSuggestion}\n");

            // Test 2: Get habits from custom prompt
            Console.WriteLine("Test 2: Getting habits from custom prompt...");
            var customPrompt = "Suggest 3 habits for improving productivity";
            var customHabits = await aiService.GetHabitsFromPrompt(customPrompt);
            Console.WriteLine($"Prompt: {customPrompt}");
            Console.WriteLine($"Result: {customHabits}\n");

            // Test 3: Multiple requests
            Console.WriteLine("Test 3: Testing multiple requests...");
            var prompts = new[]
            {
                "Suggest a habit for better sleep",
                "Suggest a habit for exercise",
                "Suggest a habit for mindfulness"
            };

            foreach (var prompt in prompts)
            {
                var response = await aiService.GetHabitsFromPrompt(prompt);
                Console.WriteLine($"Prompt: {prompt}");
                Console.WriteLine($"Response: {response}");
                Console.WriteLine("---");
            }

            Console.WriteLine("\nAll tests completed!");
        }
    }
}