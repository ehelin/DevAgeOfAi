using Microsoft.VisualBasic;
using Shared.Interfaces;
using System.Text.Json;
using shared = Shared;

namespace BLL.Ai.Services
{
    /// <summary>
    /// "My purpose is to provide the user with trackable habits. I begin with a general prompt, and I improve as I gather more goals and responses."
    /// </summary>
    public class AgentEngine
    {
        private readonly IHabitPromptService _promptService;
        private readonly IGoalMemoryService _memoryService;
        private readonly OpenAiService _aiService;
        private Timer _timer;
        private bool isRunning = false;

        public AgentEngine(IHabitPromptService promptService, IGoalMemoryService memoryService, OpenAiService aiService)
        {
            _promptService = promptService;
            _memoryService = memoryService;
            _aiService = aiService;
        }

        public void Start()
        {
            _timer = new Timer(AgentLoop, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
            isRunning = true;
        }

        private async void AgentLoop(object state)
        {
            if (isRunning)
            {
                isRunning = false;
            }
            else
            {
                return;
            }
            
            var goals = await _memoryService.GetTrackedGoalsAsync();

            // if no goals, seed with the agent's core directive
            if (!goals.Any())
            {
                await _memoryService.AddGoalAsync(shared.Constants.HABIT_TO_TRACK_PROMPT);
                goals = await _memoryService.GetTrackedGoalsAsync();
            }

            foreach (var goal in goals)
            {
                var prompt = $"The user is currently focused on the goal: {goal}. " +
                    $"Suggest {3} short, specific daily habits that would support this goal.";

                var response = await _aiService.GetHabitsFromPrompt(prompt);

                var habits = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var habit in habits)
                {
                    var cleaned = habit.TrimStart('-', '*', ' ', '1', '.', ')').Trim();
                    if (!string.IsNullOrWhiteSpace(cleaned))
                    {
                        await _memoryService.AddGoalAsync(cleaned); // Agent-generated sub-goals
                    }
                }

                await SaveSuggestedHabitsAsync(goal, response);

                isRunning = true;
            }
        }

        private async Task SaveSuggestedHabitsAsync(string goal, string response)
        {
            var suggestions = new Dictionary<string, string>();

            if (File.Exists(shared.Constants.PATH_SUGGESTED_HABITS))
            {
                var json = await File.ReadAllTextAsync(shared.Constants.PATH_SUGGESTED_HABITS);
                suggestions = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                              ?? new Dictionary<string, string>();
            }
            else
            {
                using (File.Create(shared.Constants.PATH_SUGGESTED_HABITS)) { }
            }

            suggestions[goal] = response;

            var updatedJson = JsonSerializer.Serialize(suggestions, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(shared.Constants.PATH_SUGGESTED_HABITS, updatedJson);
        }

        public void Stop()
        {
            _timer?.Dispose();
        }
    }
}
