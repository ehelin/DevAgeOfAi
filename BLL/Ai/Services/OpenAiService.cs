using Shared.Interfaces;

namespace BLL.Ai.Services
{
    public class OpenAiService : IThirdPartyAiService
    {
        private readonly IClient client;

        private const string HABIT_TO_TRACK_PROMPT = "Please suggest a habit that can be tracked";

        public OpenAiService(IEnumerable<IClient> clients)
        {
            this.client = clients.First(x => x is BLL.Ai.Clients.OpenAi.Client);
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
