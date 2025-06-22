using Shared;
using Shared.Interfaces;

namespace BLL.Ai.Services
{
    public class OpenAiService : IThirdPartyAiService
    {
        private readonly IClient client;
        private readonly IHabitPromptService habitPromptService;

        public OpenAiService(IEnumerable<IClient> clients, IHabitPromptService habitPromptService)
        {
            this.client = clients.First(x => x is BLL.Ai.Clients.OpenAi.Client);
            this.habitPromptService = habitPromptService;
        }

        public async Task<string> GetHabitToTrackSuggestion()
        {
            var augmentedPrompt = this.habitPromptService.BuildHabitPrompt(Constants.HABIT_TO_TRACK_PROMPT);
            var response = await GetSuggestion(augmentedPrompt);

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
