using Shared;
using Shared.Interfaces;
using System.Diagnostics;
using System.Text;

namespace BLL.Ai.Services
{
	public class PythonScriptService : IThirdPartyAiService, IPythonScriptService, IDisposable
	{
		private readonly string _scriptPath;
		private Process _process;
		private StreamWriter _writer;
		private StreamReader _reader;
		//private StreamReader _errorReader;

		//private string modelName = "mistral:7b";
		private string prompt = Constants.HABIT_TO_TRACK_PROMPT;
		private string model_path = "C:/temp/DevAgeTraining/Ai/AiModelRunner/fine_tuned_phi_v3/";
									 //C:\temp\DevAgeTraining\Ai\AiModelRunner/fine_tuned_phi_v3

        public PythonScriptService(string scriptPath)
		{
			_scriptPath = scriptPath ?? throw new ArgumentNullException(nameof(scriptPath));

			if (!File.Exists(_scriptPath))
			{
				throw new FileNotFoundException($"Python script not found at {_scriptPath}");
			}
		}

        public string RunModelAndReadOutput()
        {
            var response = string.Empty;

            var psi = new ProcessStartInfo
            {
                FileName = "python",
                //          C:\temp\DevAgeTraining\Ai\AiModelRunner
                Arguments = "C:\\temp\\DevAgeTraining\\Ai\\AiModelRunner\\model_runner.py",
                WorkingDirectory = "C:\\temp\\DevAgeTraining\\Ai\\AiModelRunner\\",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                process.WaitForExit();
            }

            var outputPath = "C:\\temp\\DevAegTraining\\Ai\\AiModelRunner\\output.txt";

            if (File.Exists(outputPath))
            {
                response = File.ReadAllText(outputPath);
                File.Delete(outputPath);
            }
            else
            {
                response = "Error: No output file created.";
            }

            return response;
        }

        #region IThirdPartyAiService

        public async Task<string> GetHabitToTrackSuggestion()
		{
			var result = await SendInputAsync(Constants.HABIT_TO_TRACK_PROMPT);

			return result;
		}

        #endregion

        #region IPythonScriptService

        public async Task StartAsync()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{_scriptPath}\" \"{model_path}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(_scriptPath)
            };

            _process = new Process { StartInfo = psi };

            // 🔥 Hook stderr handler BEFORE starting
            _process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                {
                    Console.WriteLine("PY STDERR: " + args.Data);
                }
            };

            _process.Start();

            _writer = _process.StandardInput;
            _reader = _process.StandardOutput;

            _process.BeginErrorReadLine(); // ⬅️ async-safe now

            await WaitForReadinessAsync();
        }


        //public async Task StartAsync()
        //{
        //	var psi = new ProcessStartInfo
        //	{
        //		FileName = "python",
        //              //Arguments = $"\"{_scriptPath}\" \"{modelName}\" \"{prompt}\"",
        //              Arguments = $"\"{_scriptPath}\" \"{model_path}\"",
        //              //Arguments = $"\"{_scriptPath}\"",
        //		RedirectStandardInput = true,
        //		RedirectStandardOutput = true,
        //		RedirectStandardError = true,
        //		UseShellExecute = false,
        //		CreateNoWindow = true,
        //              WorkingDirectory = Path.GetDirectoryName(_scriptPath)
        //          };

        //	_process = new Process { StartInfo = psi };
        //	_process.Start();

        //	_writer = _process.StandardInput;
        //	_reader = _process.StandardOutput;
        //	_errorReader = _process.StandardError;

        //	await WaitForReadinessAsync();
        //}

        //public async Task<string> SendInputAsync(string input)
        //{
        //	if (_process == null || _process.HasExited)
        //	{
        //		throw new InvalidOperationException("Python process is not running.");
        //	}

        //	await _writer.WriteLineAsync(input);
        //	await _writer.FlushAsync();

        //          // ----------------------------------------
        //          var line = await _reader.ReadLineAsync();
        //          //var sb = new StringBuilder();
        //          //string? line;
        //          //while ((line = await _reader.ReadLineAsync()) != null)
        //          //{
        //          //    if (line.Trim() == "Python exiting")
        //          //        break;

        //          //    if (!string.IsNullOrWhiteSpace(line))
        //          //    {
        //          //        sb.AppendLine(line);
        //          //    }
        //          //    else
        //          //    {
        //          //        break; // stop on blank line
        //          //    }
        //          //}

        //          //line = sb.ToString().Trim();
        //          // ----------------------------------------

        //          return line;
        //}

        public async Task<string> SendInputAsync(string input)
        {
            if (_process == null || _process.HasExited)
                throw new InvalidOperationException("Python process is not running.");

            await _writer.WriteLineAsync(input);
            await _writer.FlushAsync();

            var sb = new StringBuilder();
            string? line;

            while ((line = await _reader.ReadLineAsync()) != null)
            {
                if (line.Trim() == "<<END>>")
                    break;

                sb.AppendLine(line);
            }

            var response = sb.ToString().Trim();

            return response;
        }

        public void Stop()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (_process != null && !_process.HasExited)
			{
				_writer.WriteLine("exit");
				_writer.Flush();
				_process.WaitForExit();
			}

			_writer?.Dispose();
			_reader?.Dispose();
			//_errorReader?.Dispose();
			_process?.Dispose();
		}

        #endregion

        #region Private Methods

        //private async Task WaitForReadinessAsync()
        //{
        //	var isReady = false;
        //	while (!isReady && !_reader.EndOfStream)
        //	{
        //		var line = await _reader.ReadLineAsync();
        //		if (line != null && line.Contains("Python model ready", StringComparison.OrdinalIgnoreCase))
        //		{
        //			isReady = true;
        //		}
        //	}

        //	if (!isReady)
        //	{
        //		throw new InvalidOperationException("Python script did not become ready.");
        //	}
        //}

        private async Task WaitForReadinessAsync()
        {
            var isReady = false;
            while (!isReady && !_reader.EndOfStream)
            {
                var stdoutLine = await _reader.ReadLineAsync();
                if (!string.IsNullOrEmpty(stdoutLine))
                {
                    Console.WriteLine($"STDOUT: {stdoutLine}");
                    if (stdoutLine.Contains("Python model ready", StringComparison.OrdinalIgnoreCase))
                    {
                        isReady = true;
                        break;
                    }
                }
            }

            //// Also scan stderr just in case
            //while (!isReady && !_errorReader.EndOfStream)
            //{
            //    var stderrLine = await _errorReader.ReadLineAsync();
            //    if (!string.IsNullOrEmpty(stderrLine))
            //    {
            //        Console.WriteLine($"STDERR: {stderrLine}");
            //        if (stderrLine.Contains("Python model ready", StringComparison.OrdinalIgnoreCase))
            //        {
            //            isReady = true;
            //            break;
            //        }
            //    }
            //}

            if (!isReady)
            {
                throw new InvalidOperationException("Python script did not become ready.");
            }
        }

        #endregion
    }
}
