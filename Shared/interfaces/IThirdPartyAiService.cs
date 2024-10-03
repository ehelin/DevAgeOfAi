namespace Shared.interfaces
{
    public interface IThirdPartyAiService
    {
        Task<string> GetHabitToTrackSuggestion();
    }
}
