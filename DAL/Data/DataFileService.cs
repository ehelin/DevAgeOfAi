using System.Text.Json;
using System.Collections.Generic;
using DAL.Json.Files;
using Shared.Interfaces;
using Shared.Models;
using Shared;

namespace Bll.Data
{
    public class FileLockHelper : IDataFileService
    {
        public List<Habit> LoadHabits() => HabitDataFile.Load();
        public void SaveHabits(List<Habit> habits) => HabitDataFile.Save(habits);

        public List<HistorianState> LoadHistorianNotes() => HistorianAgentFile.Load();
        public void SaveHistorianNotes(List<HistorianState> notes) => HistorianAgentFile.Save(notes);

        public StrategistState LoadStrategistState() => StrategistAgentFile.Load();
        public void SaveStrategistState(StrategistState state) => StrategistAgentFile.Save(state);

        public List<SuggestedHabit> LoadSuggestions() => SuggestedDataFile.Load();
        public void SaveSuggestions(List<SuggestedHabit> suggestions) => SuggestedDataFile.Save(suggestions);

        public BuilderState LoadBuilderState() => BuilderAgentFile.Load();
        public void SaveBuilderState(BuilderState state) => BuilderAgentFile.Save(state);

        public void LogAgentActivity(string message)
        {
            var timestampedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            try
            {
                var path = Constants.AgentActivityLogFile;

                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                }

                File.AppendAllText(path, timestampedMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Optionally handle logging error
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }
    }
}
