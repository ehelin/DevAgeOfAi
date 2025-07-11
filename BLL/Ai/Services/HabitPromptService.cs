using Newtonsoft.Json;
using Shared.Interfaces;
using Shared.Models;
using System.Text;

namespace BLL.Ai.Services
{
    public class HabitPromptService : IHabitPromptService
    {
        private readonly List<string> _habits;
        private readonly Random _random = new();

        public HabitPromptService()
        {
            var habitFilePath = "habits.json"; 
            if (File.Exists(habitFilePath))
            {
                var json = File.ReadAllText(habitFilePath);
                var fullHabits = JsonConvert.DeserializeObject<List<Habit>>(json) ?? new List<Habit>();
                _habits = fullHabits.Select(h => h.Name).ToList(); // Extract just the names
                //_habits = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            else
            {
                File.Create(habitFilePath);
                _habits = new List<string>(); // fallback if file not found
            }
        }

        public string BuildHabitPrompt(string userPrompt, int numberOfHabits = 5)
        {
            var selectedHabits = _habits
               .OrderBy(_ => _random.Next())
               .Take(numberOfHabits)
               .ToList();

            var choices = string.Join(", ", selectedHabits);

            var sb = new StringBuilder();
            sb.AppendLine($"User prompt: \"{userPrompt}\"");
            sb.AppendLine();
            sb.AppendLine($"Choose one: {choices}");
            sb.AppendLine();
            sb.AppendLine("Return exactly one habit from the list above.");
            sb.AppendLine("Do not add dashes, punctuation, or descriptions.");
            sb.AppendLine("Return only the habit text exactly as written.");

            return sb.ToString();
        }
    }
}
