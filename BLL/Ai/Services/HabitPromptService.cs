using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using Shared.Interfaces;

namespace BLL.Ai.Services
{
    public class HabitPromptService : IHabitPromptService
    {
        private readonly List<string> _habits;
        private readonly Random _random = new();

        public HabitPromptService()
        {
            //var habitFilePath = Path.Combine("Ai", "Data", "habits.json");
            var habitFilePath = @"C:\temp\DevAgeTraining\BLL\Ai\Data\habits.json"; // TODO - make dynamic
            if (File.Exists(habitFilePath))
            {
                var json = File.ReadAllText(habitFilePath);
                _habits = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            else
            {
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
