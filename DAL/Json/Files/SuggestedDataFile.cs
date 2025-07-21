using Shared.Models;
using Shared;
using System.Text.Json;
using DAL.Json; // <-- Make sure this is included to access FileLockHelper

namespace DAL.Json.Files
{
    public static class SuggestedDataFile
    {
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 50;

        public static List<SuggestedHabit> Load()
        {
            string path = Constants.SuggestionsFile;

            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        var empty = new List<SuggestedHabit>();
                        Save(empty);
                        return empty;
                    }

                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(json))
                        return new List<SuggestedHabit>();

                    return JsonSerializer.Deserialize<List<SuggestedHabit>>(json) ?? new List<SuggestedHabit>();
                }
                catch (IOException)
                {
                    Thread.Sleep(RetryDelayMs);
                }
            }

            return new List<SuggestedHabit>();
        }

        public static void Save(List<SuggestedHabit> habits)
        {
            string path = Constants.SuggestionsFile;

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var tempPath = path + ".tmp";

            var json = JsonSerializer.Serialize(habits, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(tempPath, json);

            try
            {
                File.Copy(tempPath, path, overwrite: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to overwrite {path}: {ex.Message}");
            }

            File.Delete(tempPath);
        }
    }
}
