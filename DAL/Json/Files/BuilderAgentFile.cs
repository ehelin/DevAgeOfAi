using Shared.Models;
using Shared;
using System.Text.Json;

namespace DAL.Json.Files
{
    public static class BuilderAgentFile
    {
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 50;

        public static BuilderState Load()
        {
            string path = Constants.BuilderStateFile;

            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        var empty = new BuilderState();
                        Save(empty);
                        return empty;
                    }

                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(json))
                        return new BuilderState();

                    return JsonSerializer.Deserialize<BuilderState>(json) ?? new BuilderState();
                }
                catch (IOException)
                {
                    Thread.Sleep(RetryDelayMs);
                }
            }

            return new BuilderState();
        }

        public static void Save(BuilderState builderState)
        {
            string path = Constants.BuilderStateFile;

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var tempPath = path + ".tmp";

            var json = JsonSerializer.Serialize(builderState, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(tempPath, json);
            File.Copy(tempPath, path, overwrite: true);
            File.Delete(tempPath);
        }
    }
}
