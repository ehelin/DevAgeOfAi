using BLL.Ai.Clients.MicrosoftAi;
using Shared;
using Shared.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace BLL.Ai.Services
{
    public class GoalMemoryService : IGoalMemoryService
    {
        private readonly string _filePath = @"C:\temp\DevAgeTraining\BLL\Ai\Data\goals.json"; // TODO - make dynamic

        public async Task<List<string>> GetTrackedGoalsAsync()
        {
            if (!File.Exists(_filePath))
            {
                File.Create(_filePath);
                return new List<string>();
            }

            var json = await File.ReadAllTextAsync(_filePath);
            if (string.IsNullOrEmpty(json))
            {
                return new List<string>();
            }
            else
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
        }

        public async Task AddGoalAsync(string goal)
        {
            var goals = await GetTrackedGoalsAsync();

            if (!goals.Contains(goal, StringComparer.OrdinalIgnoreCase))
            {
                goals.Add(goal);
                var json = JsonSerializer.Serialize(goals, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_filePath, json);
            }
        }
    }
}
