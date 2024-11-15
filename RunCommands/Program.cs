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

            // Read and print the initial "ready" message
            Console.WriteLine(reader.ReadLine());

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
                    if (!string.IsNullOrWhiteSpace(outputLine))
                    {
                        try
                        {
                            // Parse the output line as JSON
                            var json = JObject.Parse(outputLine);

                            // Extract and display JSON fields
                            var prompt = json["prompt"]?.ToString();
                            var response = json["response"]?.ToString();

                            Console.WriteLine($"Python stdout (parsed):");
                            Console.WriteLine($"  Prompt: {prompt}");
                            Console.WriteLine($"  Response: {response}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing JSON: {ex.Message}");
                            Console.WriteLine($"Raw Output: {outputLine}");
                        }
                    }
                }

                // Read Python's stderr
                while (!errorReader.EndOfStream)
                {
                    string errorLine = errorReader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(errorLine))
                    {
                        Console.WriteLine($"Python stderr: {errorLine}");
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
