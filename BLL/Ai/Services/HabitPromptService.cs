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
            var habitFilePath = @"C:\temp\DevAgeTraining\BLL\Ai\Data\habits.json"; // TODO - make dynamic
            if (File.Exists(habitFilePath))
            {
                var json = File.ReadAllText(habitFilePath);
                _habits = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            else
            {
                File.Create(habitFilePath);
                _habits = new List<string>(); // fallback if file not found
            }
        }

        public string BuildHabitPrompt(string userPrompt, int numberOfHabits = 5)
        {
            return $"The user is currently focused on the goal: {userPrompt}. " +
                    $"Suggest {numberOfHabits} short, specific daily habits that would support this goal.";
        }
    }
}
