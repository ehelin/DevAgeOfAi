using HabitTracker.Models;
using Shared.Models;
using System.Text.Json;

namespace BLL.Ai.Agents
{
    public class HistorianAgent : BaseAgent
    {
        private const string StateFile = "historian.json";

        private HistorianState state = new();

        public override void Start()
        {
            _timer = new System.Threading.Timer(AgentLoop, null, System.TimeSpan.Zero, TimeSpan.FromSeconds(15));
            isRunning = true;
        }

        public override void Stop()
        {
            _timer?.Dispose();
        }

        public async void AgentLoop(object state)
        {
            if (isRunning)
            {
                isRunning = false;
            }
            else
            {
                return;
            }

            var habits = LoadData();
            if (habits != null && habits.Count() > 0)
            {
                habits = AnalyzePatterns(habits);
                SaveState();
                UpdateSuggestions();
            }

            isRunning = true;
        }

        private List<Habit> LoadData()
        {
            var currentHabits = new List<Habit>();

            if (File.Exists(DataFile))
            {
                var json = File.ReadAllText(DataFile); 
                
                if (!string.IsNullOrWhiteSpace(json))
                {
                    currentHabits = JsonSerializer.Deserialize<List<Habit>>(json) ?? new List<Habit>();
                }
            }
            // If file doesn't exist, create it
            else 
            {
                File.Create(DataFile).Close();
            }

            return currentHabits;
        }

        private List<Habit> AnalyzePatterns(List<Habit> habits)
        {
            // Example logic: find habits with consistent streaks or repeated failures
            foreach (var habit in habits)
            {
                var history = state.HabitHistory.FirstOrDefault(h => h.Id == habit.Id);
                if (history == null)
                {
                    history = new HabitPattern { Id = habit.Id, Name = habit.Name };
                    state.HabitHistory.Add(history);
                }

                // Update pattern stats (placeholder logic)
                history.SuccessRate = CalculateSuccessRate(habit);
            }

            return habits;
        }

        private void SaveState()
        {
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });

            if (!File.Exists(DataFile))
            {
                File.Create(DataFile).Close();
            }

            File.WriteAllText(StateFile, json);
        }

        private void UpdateSuggestions()
        {
            var strongHabits = state.HabitHistory
                .Where(h => h.SuccessRate > 0.8)
                .Select(h => new SuggestedHabit { Name = h.Name, Reason = "High success rate" })
                .ToList();

            var json = JsonSerializer.Serialize(strongHabits, new JsonSerializerOptions { WriteIndented = true });
            if (string.IsNullOrWhiteSpace(json) || json == "[]")
                return;

            if (!File.Exists(SuggestionsFile))
            {
                File.Create(SuggestionsFile).Close();
            }

            File.WriteAllText(SuggestionsFile, json);
        }

        private double CalculateSuccessRate(Habit habit)
        {
            if (habit.TotalAttempts == 0) return 0;
            return (double)habit.SuccessCount / habit.TotalAttempts;
        }
    }
}
