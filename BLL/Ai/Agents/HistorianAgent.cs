using Shared.Interfaces;
using Shared.Models;
using System.Text;

namespace BLL.Ai.Agents
{
    public class HistorianAgent : BaseAgent
    {
        private HistorianState state = new();

        public HistorianAgent(IDataFileService dataFileService) : base()
        {
            this.dataFileService = dataFileService;
        }

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
            if (!ShouldContinueRunning())
                return;

            this.sb = new StringBuilder();
            var habits = dataFileService.LoadHabits();

            this.sb.AppendLine($"");
            this.sb.AppendLine($"=======================================");
            this.sb.AppendLine($"HistorianAgent {DateTime.Now}");
            this.sb.AppendLine($"HistorianAgent: Loaded files - habits: {habits.Count()}");
            if (habits != null && habits.Count() > 0)
            {
                var agentState = AnalyzePatterns(habits);
                var list = new List<HistorianState>() { agentState };
                this.dataFileService.SaveHistorianNotes(list);

                var activeHabitNames = habits
                    .Where(h => h.Status == "Active")
                    .Select(h => h.Name)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                var strongHabits = agentState.HabitHistory
                    .Where(h => h.SuccessRate > 0.8 && !activeHabitNames.Contains(h.Name))
                    .Select(h => new SuggestedHabit { Name = h.Name, Reason = "High success rate" })
                    .ToList();

                if (strongHabits != null && strongHabits.Count() > 0)
                {
                    this.sb.AppendLine($"HistorianAgent: saving strong Habits: {strongHabits.Count()}...");
                    this.dataFileService.SaveSuggestions(strongHabits);
                }
            }

            this.sb.AppendLine($"");
            this.sb.AppendLine($"");

            this.dataFileService.LogAgentActivity(sb.ToString());

            isRunning = true;
        }
        
        private HistorianState AnalyzePatterns(List<Habit> habits)
        {
            // Example logic: find habits with consistent streaks or repeated failures
            foreach (var habit in habits)
            {
                this.sb.AppendLine($"HistorianAgent: Analyzing habit: {habit.Name}...");
                var history = state.HabitHistory.FirstOrDefault(h => h.Id == habit.Id);
                if (history == null)
                {
                    history = new HabitPattern { Id = habit.Id, Name = habit.Name };
                    state.HabitHistory.Add(history);
                    this.sb.AppendLine($"HistorianAgent: Saving habit: {habit.Name} history {history.Name} ...");
                }

                // Update pattern stats (placeholder logic)
                history.SuccessRate = CalculateSuccessRate(habit);

                this.sb.AppendLine($"HistorianAgent: habit: {habit.Name} calculated success rate {history.SuccessRate}...");
            }

            return state;
        }

        private double CalculateSuccessRate(Habit habit)
        {
            if (habit.TotalAttempts == 0) return 0;
            return (double)habit.SuccessCount / habit.TotalAttempts;
        }
    }
}
