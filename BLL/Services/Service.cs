using BLL.Services.OpenAi;
using Shared.interfaces;
using Shared.Interfaces;

namespace BLL.Services
{
    public class Service : IThirdPartyAiService
    {
        private readonly IClient client;

        private const string HABIT_TO_TRACK_PROMPT = "Please suggest a habit that can be tracked";

        public Service(IClient client)
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
