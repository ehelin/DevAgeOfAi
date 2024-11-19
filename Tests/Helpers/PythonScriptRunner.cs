using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Helpers
{
    public class PythonScriptRunner : IDisposable
    {
        private readonly string _scriptPath;
        private Process _process;
        private StreamWriter _writer;
        private StreamReader _reader;
        private StreamReader _errorReader;

        public PythonScriptRunner(string scriptPath)
        {
            _scriptPath = scriptPath ?? throw new ArgumentNullException(nameof(scriptPath));

            if (!File.Exists(_scriptPath))
            {
                throw new FileNotFoundException($"Python script not found at {_scriptPath}");
            }

            StartPythonProcess();
        }

        private void StartPythonProcess()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{_scriptPath}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _process = new Process { StartInfo = psi };
            _process.Start();

            _writer = _process.StandardInput;
            _reader = _process.StandardOutput;
            _errorReader = _process.StandardError;

            WaitForReadiness();
        }

        private void WaitForReadiness()
        {
            var isReady = false;
            while (!isReady && !_reader.EndOfStream)
            {
                var line = _reader.ReadLine();
                if (line != null && line.Contains("Running in main mode.", StringComparison.OrdinalIgnoreCase))
                {
                    isReady = true;
                }
            }

            if (!isReady)
            {
                throw new InvalidOperationException("Python script did not become ready.");
            }
        }

        public async Task<string> SendInputAsync(string input)
        {
            if (_process == null || _process.HasExited)
            {
                throw new InvalidOperationException("Python process is not running.");
            }

            // Send input to the Python script
            await _writer.WriteLineAsync(input);
            await _writer.FlushAsync();

            // Read the response
            var responseBuilder = new StringBuilder();
            while (!_reader.EndOfStream)
            {
                var line = await _reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line))
                    break;

                responseBuilder.AppendLine(line);

                // Exit the loop if the response indicates completion
                if (line.Contains("}", StringComparison.OrdinalIgnoreCase))
                    break;
            }

            return responseBuilder.ToString();
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
            _errorReader?.Dispose();
            _process?.Dispose();
        }
    }
}
