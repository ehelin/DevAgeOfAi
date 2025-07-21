using Shared.Models;
using Shared;
using System.Text.Json;

namespace DAL.Json.Files
{
    public static class HistorianAgentFile
    {
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 50;

        public static List<HistorianState> Load()
        {
            string path = Constants.HistorianStateFile;

            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        var empty = new List<HistorianState>();
                        Save(empty);
                        return empty;
                    }

                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(json))
                        return new List<HistorianState>();

                    return JsonSerializer.Deserialize<List<HistorianState>>(json) ?? new List<HistorianState>();
                }
                catch (IOException)
                {
                    Thread.Sleep(RetryDelayMs);
                }
            }

            return new List<HistorianState>();
        }

        public static void Save(List<HistorianState> historianStates)
        {
            string path = Constants.HistorianStateFile;

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var tempPath = path + ".tmp";

            var json = JsonSerializer.Serialize(historianStates, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(tempPath, json);
            File.Copy(tempPath, path, overwrite: true);
            File.Delete(tempPath);
        }
    }
}
