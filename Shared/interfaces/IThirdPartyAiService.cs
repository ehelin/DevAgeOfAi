namespace Shared.Interfaces
{
    public interface IThirdPartyAiService
    {
        Task<string> GetHabitToTrackSuggestion();
    }
}
