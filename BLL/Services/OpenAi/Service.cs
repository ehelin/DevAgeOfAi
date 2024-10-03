using Shared.interfaces;

namespace BLL.Services.OpenAi
{
    public class Service : IThirdPartyAiService
    {
        private readonly Client client;

        private const string HABIT_TO_TRACK_PROMPT = "Please suggest a habit that can be tracked";

        public Service(Client client)
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
            var result = await this.client.GetCompletionAsync(prompt);

            return result;
        }

        #endregion
    }
}
