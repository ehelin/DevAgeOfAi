using Shared.Interfaces;
using Shared.Models;
using System.Text;
using System.Text.Json;

namespace BLL.Ai.Agents
{
    public class StrategistAgent : BaseAgent
    {
        private HistorianState state = new();

        public StrategistAgent(IDataFileService dataFileService) : base()
        {
            this.dataFileService = dataFileService;
        }

        public override void Start()
        {
            _timer = new System.Threading.Timer(AgentLoop, null, System.TimeSpan.Zero, TimeSpan.FromSeconds(15));  //60
            isRunning = true;
        }

        public override void Stop()
        {
            _timer?.Dispose();
        }

        public async void AgentLoop(object state)
        {
            if (!ShouldContinueRunning())
                return;

            this.sb = new StringBuilder();
            var habits = dataFileService.LoadHabits();

            var historianInsights = dataFileService.LoadHistorianNotes();
            var strategistState = dataFileService.LoadStrategistState();
            var strategistStateValue = strategistState != null ? "true" : "false";

            this.sb.AppendLine($"");
            this.sb.AppendLine($"=======================================");
            this.sb.AppendLine($"StrategistAgent {DateTime.Now}:");
            this.sb.AppendLine($"StrategistAgent: Loaded files - habits: {habits.Count()}/ strategistState: {strategistStateValue} / historianState: {historianInsights.Count()}");

            var suggestions = new List<string>();
            var strongHistory = historianInsights
                .SelectMany(h => h.HabitHistory)
                .Where(h => h.SuccessRate > 0.8)
                .ToDictionary(h => h.Name, StringComparer.OrdinalIgnoreCase);

            // Example logic
            foreach (var habit in habits)
            {
                this.sb.AppendLine($"StrategistAgent: Analyzing habit: {habit.Name}...");
                bool isHistoricallyStrong = strongHistory.ContainsKey(habit.Name);

                if (habit.Streak >= 5 && !habit.IsPaused && !isHistoricallyStrong)
                {
                    suggestions.Add($"Pause habit: {habit.Name}");
                    sb.AppendLine($"StrategistAgent: Pausing habit: {habit.Name}...");
                }
                else if (habit.Streak == 0 && !habit.IsPaused && isHistoricallyStrong)
                {
                    suggestions.Add($"Resume strong past habit: {habit.Name}");
                    sb.AppendLine($"StrategistAgent: Resuming historically strong habit: {habit.Name}...");
                }
                else if (habit.Streak == 0 && !habit.IsPaused)
                {
                    suggestions.Add($"Reinforce habit: {habit.Name}");
                    sb.AppendLine($"StrategistAgent: Reinforcing habit: {habit.Name}...");
                }
            }

            this.sb.AppendLine($"");
            this.sb.AppendLine($"");

            // Save strategist state and suggestions
            strategistState.GeneratedAt = DateTime.UtcNow;
            strategistState.Suggestions = suggestions;

            this.dataFileService.SaveStrategistState(strategistState);
            this.dataFileService.LogAgentActivity(sb.ToString());

            isRunning = true;
        }
    }
}
