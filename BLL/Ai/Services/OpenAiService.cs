﻿using Shared;
using Shared.Interfaces;
using System.Text;
using System.Text.Json;

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

        public async Task<string> GetHabitsFromPrompt(string prompt)
        {
            return await client.GetCompletionAsync(prompt);
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
