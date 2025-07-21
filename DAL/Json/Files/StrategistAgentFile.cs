using Shared.Models;
using Shared;
using System.Text.Json;

namespace DAL.Json.Files
{
    public static class StrategistAgentFile
    {
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 50;

        public static StrategistState Load()
        {
            string path = Constants.StrategistStateFile;

            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        var empty = new StrategistState();
                        Save(empty); // create empty file
                        return empty;
                    }

                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(json))
                        return new StrategistState();

                    return JsonSerializer.Deserialize<StrategistState>(json) ?? new StrategistState();
                }
                catch (IOException)
                {
                    Thread.Sleep(RetryDelayMs);
                }
            }

            return new StrategistState(); // fallback
        }

        public static void Save(StrategistState strategistState)
        {
            string path = Constants.StrategistStateFile;

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var tempPath = path + ".tmp";

            var json = JsonSerializer.Serialize(strategistState, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(tempPath, json);
            File.Copy(tempPath, path, overwrite: true);
            File.Delete(tempPath);
        }
    }
}
