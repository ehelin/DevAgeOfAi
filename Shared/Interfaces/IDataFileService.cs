using Shared.Models;

namespace Shared.Interfaces
{
    public interface IDataFileService
    {
        List<Habit> LoadHabits();
        void SaveHabits(List<Habit> habits);

        List<HistorianState> LoadHistorianNotes();
        void SaveHistorianNotes(List<HistorianState> notes);

        StrategistState LoadStrategistState();
        void SaveStrategistState(StrategistState state);

        List<SuggestedHabit> LoadSuggestions();
        void SaveSuggestions(List<SuggestedHabit> suggestions);

        BuilderState LoadBuilderState();
        void SaveBuilderState(BuilderState state);
    }
}
