using Shared.Models;
using Shared;
using System.Text.Json;

namespace DAL.Json.Files
{
    public static class HabitDataFile
    {
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 50;

        public static List<Habit> Load()
        {
            string path = Constants.DataFile;

            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        var empty = new List<Habit>();
                        Save(empty);
                        return empty;
                    }

                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(json))
                        return new List<Habit>();

                    return JsonSerializer.Deserialize<List<Habit>>(json) ?? new List<Habit>();
                }
                catch (IOException)
                {
                    Thread.Sleep(RetryDelayMs);
                }
            }

            return new List<Habit>(); // fallback
        }

        public static void Save(List<Habit> habits)
        {
            string path = Constants.DataFile;

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var tempPath = path + ".tmp";

            var json = JsonSerializer.Serialize(habits, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(tempPath, json);
            File.Copy(tempPath, path, overwrite: true);
            File.Delete(tempPath);
        }
    }
}
