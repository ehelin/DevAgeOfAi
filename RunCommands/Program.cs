using BLL.Ai.Services;
using Shared.Interfaces;
using System.Diagnostics;

class Program
{
    static async Task Main(string[] args)
    {
        // Define the script path
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
