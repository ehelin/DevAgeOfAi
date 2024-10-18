using BLL.Ai.Clients.MicrosoftAi;
using Shared.Interfaces;

namespace BLL.Ai.Services
{
    public class MicrosoftAiService : IThirdPartyAiService
    {
        private readonly IClient client;

        private const string HABIT_TO_TRACK_PROMPT = "Please suggest a habit that can be tracked";

        public MicrosoftAiService(IClient client)
        {
            this.client = client;
        }

        public async Task<string> GetHabitToTrackSuggestion()
        {
            var response = await GetSuggestion(HABIT_TO_TRACK_PROMPT);

            return response;
        }

        #region Private Methods

        private async Task<string> GetSuggestion(string prompt)
        {
            var result = await client.GetCompletionAsync(prompt);

            return result;
        }

        #endregion
    }
}
