namespace Shared.Interfaces
{
    public interface IThirdPartyAiService
    {
        Task<string> GetHabitToTrackSuggestion();
        Task<string> GetHabitsFromPrompt(string prompt);
    }
}
