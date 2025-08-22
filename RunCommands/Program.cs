using BLL.Ai.Services;
using RunCommands;
using Shared.Interfaces;
using System.Diagnostics;

class Program
{
    static async Task Main(string[] args)
    {
       // if (args.Length > 0 && args[0] == "test-python-ai")
        //{
            // Run the new Python AI client test
            await TestPythonAiClient.RunTest();
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
            return;
        //}

        // Original code for testing PythonScriptService
        var scriptPath = "C:\\temp\\New folder\\Ai\\AiModelRunner\\PythonApplication1.py";
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{scriptPath}\"",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };


        IPythonScriptService pythonService = new PythonScriptService(scriptPath);

        try
        {
            await pythonService.StartAsync();

            Console.WriteLine("Python script is ready!");

            while (true)
            {
                Console.Write("You: ");
                var userInput = Console.ReadLine();

                if (userInput?.ToLower() == "exit")
                {
                    pythonService.Stop();
                    Console.WriteLine("Exiting the chat. Goodbye!");
                    break;
                }

                var response = await pythonService.SendInputAsync(userInput);
                Console.WriteLine($"Response: {response}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
