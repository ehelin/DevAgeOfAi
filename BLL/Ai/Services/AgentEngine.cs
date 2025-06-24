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

        private readonly string _filePath = @"C:\temp\DevAgeTraining\BLL\Ai\Data\suggestedHabits.json"; // TODO - make dynamic

        public AgentEngine(IHabitPromptService promptService, IGoalMemoryService memoryService, OpenAiService aiService)
        {
            _promptService = promptService;
            _memoryService = memoryService;
            _aiService = aiService;
        }

        public void Start()
        {
            _timer = new Timer(AgentLoop, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
        }

        private async void AgentLoop(object state)
        {
            var goals = await _memoryService.GetTrackedGoalsAsync();

            // if no goals, seed with the agent's core directive
            if (!goals.Any())
            {
                await _memoryService.AddGoalAsync(shared.Constants.HABIT_TO_TRACK_PROMPT);
                goals = await _memoryService.GetTrackedGoalsAsync();
            }

            foreach (var goal in goals)
            {
                var prompt = _promptService.BuildHabitPrompt(goal);
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
            }
        }

        private async Task SaveSuggestedHabitsAsync(string goal, string response)
        {
            var suggestions = new Dictionary<string, string>();

            if (File.Exists(_filePath))
            {
                var json = await File.ReadAllTextAsync(_filePath);
                suggestions = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                              ?? new Dictionary<string, string>();
            }
            else
            {
                File.Create(_filePath);
            }

            suggestions[goal] = response;

            var updatedJson = JsonSerializer.Serialize(suggestions, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, updatedJson);
        }

        public void Stop()
        {
            _timer?.Dispose();
        }
    }
}
