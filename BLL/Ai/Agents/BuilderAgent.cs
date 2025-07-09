using Shared.Interfaces;
using Shared.Models;
using System.Text.Json;

namespace BLL.Ai.Agents
{
    public class BuilderAgent : BaseAgent
    {
        private HistorianState state = new();

        public BuilderAgent(IDataFileService dataFileService) : base()
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

            var habits = dataFileService.LoadHabits();
            var strategistState = dataFileService.LoadStrategistState();
            var builderState = dataFileService.LoadBuilderState() ?? new BuilderState();

            if (strategistState?.Suggestions == null || strategistState.Suggestions.Count == 0)
            {
                isRunning = true;
                return;
            }

            foreach (var suggestion in strategistState.Suggestions)
            {
                bool applied = false;

                if (suggestion.StartsWith("Pause habit:"))
                {
                    var habitName = suggestion.Replace("Pause habit:", "").Trim();
                    var habit = habits.FirstOrDefault(h => h.Name.Equals(habitName, StringComparison.OrdinalIgnoreCase));
                    if (habit != null && !habit.IsPaused)
                    {
                        habit.IsPaused = true;
                        habit.LastUpdated = DateTime.UtcNow;
                        applied = true;
                    }
                }
                else if (suggestion.StartsWith("Reinforce habit:"))
                {
                    var habitName = suggestion.Replace("Reinforce habit:", "").Trim();
                    var habit = habits.FirstOrDefault(h => h.Name.Equals(habitName, StringComparison.OrdinalIgnoreCase));
                    if (habit != null)
                    {
                        habit.Streak += 1;
                        habit.LastUpdated = DateTime.UtcNow;
                        applied = true;
                    }
                }

                if (applied)
                    builderState.AppliedSuggestions.Add(suggestion);
                else
                    builderState.SkippedSuggestions.Add(suggestion);
            }

            builderState.LastRun = DateTime.UtcNow;

            strategistState.Suggestions.Clear();

            dataFileService.SaveHabits(habits);
            dataFileService.SaveStrategistState(strategistState);
            dataFileService.SaveBuilderState(builderState);

            isRunning = true;
        }
    }
}
