using Shared.Interfaces;
using Shared.Models;
using System.Text;
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

            this.sb = new StringBuilder();
            var strategistStateValue = strategistState != null ? "true" : "false";
            var builderStateValue = builderState != null ? "true" : "false";

            this.sb.AppendLine($"");
            this.sb.AppendLine($"=======================================");
            this.sb.AppendLine($"BuilderAgent {DateTime.Now}:");
            this.sb.AppendLine($"BuilderAgent - Loaded files - habits: {habits.Count()}/ strategistState: {strategistStateValue} / builderState: {builderStateValue}");

            if (strategistState?.Suggestions == null || strategistState.Suggestions.Count == 0)
            {
                isRunning = true;
                this.dataFileService.LogAgentActivity(sb.ToString());
                return;
            }

            foreach (var suggestion in strategistState.Suggestions)
            {
                this.sb.AppendLine($"BuilderAgent: Analyzing suggestion: {suggestion}...");
                bool applied = false;

                if (suggestion.StartsWith("Pause habit:"))
                {
                    var habitName = suggestion.Replace("Pause habit:", "").Trim();
                    var habit = habits.FirstOrDefault(h => h.Name.Equals(habitName, StringComparison.OrdinalIgnoreCase));
                    if (habit != null && !habit.IsPaused)
                    {
                        habit.IsPaused = true;
                        habit.LastUpdated = DateTime.Now;
                        applied = true;

                        this.sb.AppendLine($"BuilderAgent: Analyzing suggestion: {suggestion} is paused {habit.LastUpdated}");
                    }
                }
                else if (suggestion.StartsWith("Reinforce habit:"))
                {
                    var habitName = suggestion.Replace("Reinforce habit:", "").Trim();
                    var habit = habits.FirstOrDefault(h => h.Name.Equals(habitName, StringComparison.OrdinalIgnoreCase));
                    if (habit != null)
                    {
                        habit.Streak += 1;
                        habit.LastUpdated = DateTime.Now;
                        applied = true;
                        this.sb.AppendLine($"BuilderAgent: Analyzing suggestion: {suggestion} is reinforced {habit.Streak}");
                    }
                }

                if (applied)
                {
                    builderState.AppliedSuggestions.Add(suggestion);
                    this.sb.AppendLine($"BuilderAgent: Saving applied suggestion: {suggestion}");
                }
                else
                {
                    builderState.SkippedSuggestions.Add(suggestion);
                    this.sb.AppendLine($"BuilderAgent: Saving skipped suggestion: {suggestion}");
                }
            }

            builderState.LastRun = DateTime.Now;

            strategistState.Suggestions.Clear();

            dataFileService.SaveHabits(habits);
            dataFileService.SaveStrategistState(strategistState);
            dataFileService.SaveBuilderState(builderState);

            this.dataFileService.LogAgentActivity(sb.ToString());

            isRunning = true;
        }
    }
}
