using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;

class Program
{
	static void Main(string[] args)
	{
		var psi = new ProcessStartInfo
		{
			FileName = "python",
			Arguments = "C:\\Users\\QuesoCheese\\source\\repos\\PythonApplication1\\PythonApplication1.py", // TODO - how to make the arguments path relative?
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		using var process = new Process { StartInfo = psi };
		process.Start();

		// Read initial ready message from Python
		var reader = process.StandardOutput;
		var writer = process.StandardInput;
		Console.WriteLine(reader.ReadLine());

		while (true)
		{
			Console.Write("You: ");
			var userInput = Console.ReadLine();

			// Exit condition
			if (userInput?.ToLower() == "exit")
			{
				writer.WriteLine("exit");
				writer.Flush();
				Console.WriteLine("Exiting the chat. Goodbye!");
				break;
			}

			// Send input to Python
			writer.WriteLine(userInput);
			writer.Flush();

			// Read response from Python
			var responseJson = reader.ReadLine();
			if (responseJson != null)
			{
				// Parse JSON response
				var responseObj = JObject.Parse(responseJson);
				var response = responseObj["response"]?.ToString();
				Console.WriteLine($"Model: {response}\n");
			}
		}

		process.WaitForExit();
	}
}
