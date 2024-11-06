using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

#pragma warning disable CS8601 // Possible null reference assignment.

namespace AiModelApi
{
	/// <summary>
	/// C# wrapper around python script in AiModelRunner Python project inside this solution.
	/// 
	/// NOTE: This is NOT meant to be a production system and is only for play AI model exploration w/Taspa chat app.
	/// </summary>
	public class PythonRunner
	{
		public static string PythonPath;
		public static string ScriptPath;

		private static bool exit = false;

		/// <summary>
		/// Message/response for interacting w/Python script.  Not ideal, but works for now.
		/// </summary>
		public static string Message = string.Empty;
		public static string Response = string.Empty;
		public static void BackgroundProcessThreadMethod()
		{
			using (Process process = new Process())
			{
				process.StartInfo.FileName = PythonPath;
				process.StartInfo.Arguments = ScriptPath;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.CreateNoWindow = true;

				process.Start();

				using (StreamWriter sw = process.StandardInput)
				using (StreamReader sr = process.StandardOutput)
				{
					// Start a separate thread to capture standard output
					Thread outputThread = new Thread(() =>
					{
						try
						{
							while (!sr.EndOfStream)
							{
								string line = sr.ReadLine();
								if (line != null)
								{
									Response = line;
									Console.WriteLine("Python Output: " + line);
								}
							}
						}
						catch (IOException)
						{
							Console.WriteLine("Output stream closed.");
						}
					});
					outputThread.Start();

					try
					{
						while (!exit && !process.HasExited)
						{
							if (!string.IsNullOrEmpty(Message))
							{
								sw.WriteLine(Message);
								Message = string.Empty;
							}
							Thread.Sleep(500);
						}
					}
					catch (IOException)
					{
						Console.WriteLine("Input stream closed.");
					}
					finally
					{
						if (!process.HasExited)
						{
							sw.WriteLine("exit");
						}
					}

					outputThread.Join();
				}

				process.WaitForExit();
			}
		}

	}
}