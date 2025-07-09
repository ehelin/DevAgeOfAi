using Shared.Interfaces;
using Shared.Models;

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

            var habits = dataFileService.LoadHabits();
            if (habits != null && habits.Count() > 0)
            {
                var agentState = AnalyzePatterns(habits);
                var list = new List<HistorianState>() { agentState };
                this.dataFileService.SaveHistorianNotes(list);

                var strongHabits = agentState.HabitHistory
                    .Where(h => h.SuccessRate > 0.8)
                    .Select(h => new SuggestedHabit { Name = h.Name, Reason = "High success rate" })
                    .ToList();

                if (strongHabits != null && strongHabits.Count() > 0)
                {
                    this.dataFileService.SaveSuggestions(strongHabits);
                }
            }

            isRunning = true;
        }
        
        private HistorianState AnalyzePatterns(List<Habit> habits)
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

            return state;
        }

        private double CalculateSuccessRate(Habit habit)
        {
            if (habit.TotalAttempts == 0) return 0;
            return (double)habit.SuccessCount / habit.TotalAttempts;
        }
    }
}
