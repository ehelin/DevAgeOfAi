using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        // Define the script path
        var scriptPath = "C:\\temp\\New folder\\Ai\\AiModelRunner\\PythonApplication1.py";

        if (!File.Exists(scriptPath))
        {
            Console.WriteLine($"Error: Python script not found at {scriptPath}");
            return;
        }

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

        using var process = new Process { StartInfo = psi };

        try
        {
            process.Start();

            // Create readers for stdout and stderr
            var reader = process.StandardOutput;
            var errorReader = process.StandardError;
            var writer = process.StandardInput;

            Console.WriteLine("Starting Python script. Waiting for it to be ready...");

            // Wait for "Running in main mode." message
            bool isReady = false;
            while (!isReady)
            {
                if (!reader.EndOfStream)
                {
                    var outputLine = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(outputLine))
                    {
                        if (outputLine.Contains("Python model ready", StringComparison.OrdinalIgnoreCase))
                        {
                            isReady = true;
                        }
                    }
                }
            }

            Console.WriteLine("Python script is ready!");

            while (true)
            {
                Console.Write("You: ");
                var userInput = Console.ReadLine();

                if (userInput?.ToLower() == "exit")
                {
                    writer.WriteLine("exit");
                    writer.Flush();
                    Console.WriteLine("Exiting the chat. Goodbye!");
                    break;
                }

                // Send user input to Python
                writer.WriteLine(userInput);
                writer.Flush();

                // Read Python's stdout
                while (!reader.EndOfStream)
                {
                    string outputLine = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(outputLine) && outputLine.Contains("Please respond to the following question independently:"))
                    {
                        try
                        {
                            // Parse the output line as JSON
                            var json = JObject.Parse(outputLine);

                            // Extract and display JSON fields
                            var response = json["response"]?.ToString();

                            var subStringStart = "Answer\n";
                            var filteredResponse = response.Substring(response.IndexOf(subStringStart) + subStringStart.Length);

                            Console.WriteLine($"Response: {filteredResponse}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing JSON: {ex.Message}");
                            Console.WriteLine($"Raw Output: {outputLine}");
                        }
                    }
                }
            }

            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            process.Close();
        }
    }
}
