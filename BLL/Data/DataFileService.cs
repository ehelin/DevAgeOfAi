using System.Text.Json;
using System.Collections.Generic;
using DAL.Json.Files;
using Shared.Interfaces;
using Shared.Models;

namespace Shared.Utilties
{
    public class DataFileService : IDataFileService
    {
        public List<Habit> LoadHabits() => HabitDataFile.Load();
        public void SaveHabits(List<Habit> habits) => HabitDataFile.Save(habits);

        public List<HistorianState> LoadHistorianNotes() => HistorianAgentFile.Load();
        public void SaveHistorianNotes(List<HistorianState> notes) => HistorianAgentFile.Save(notes);

        //public StrategistState LoadStrategistState() => StrategistFile.Load();
        //public void SaveStrategistState(StrategistState state) => StrategistFile.Save(state);

        public List<SuggestedHabit> LoadSuggestions() => SuggestedDataFile.Load();
        public void SaveSuggestions(List<SuggestedHabit> suggestions) => SuggestedDataFile.Save(suggestions);
    }
}
