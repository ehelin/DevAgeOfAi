using HabitTracker.Models;
using Shared.Interfaces;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLL.Ai.Agents
{
    public class HistorianAgent : IAgent
    {
        private const string DataFile = "data.json";
        private const string SuggestionsFile = "suggestedHabits.json";
        private const string StateFile = "historian.json";

        private List<Habit> currentHabits = new();
        private HistorianState state = new();

        public void Run()
        {
            LoadData();
            AnalyzePatterns();
            SaveState();
            UpdateSuggestions();
        }

        private void LoadData()
        {
            if (File.Exists(DataFile))
            {
                var json = File.ReadAllText(DataFile);
                currentHabits = JsonSerializer.Deserialize<List<Habit>>(json) ?? new();
            }
        }

        private void AnalyzePatterns()
        {
            // Example logic: find habits with consistent streaks or repeated failures
            foreach (var habit in currentHabits)
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
        }

        private void SaveState()
        {
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(StateFile, json);
        }

        private void UpdateSuggestions()
        {
            var strongHabits = state.HabitHistory
                .Where(h => h.SuccessRate > 0.8)
                .Select(h => new SuggestedHabit { Name = h.Name, Reason = "High success rate" })
                .ToList();

            var json = JsonSerializer.Serialize(strongHabits, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SuggestionsFile, json);
        }

        private double CalculateSuccessRate(Habit habit)
        {
            if (habit.TotalAttempts == 0) return 0;
            return (double)habit.SuccessCount / habit.TotalAttempts;
        }
    }
}
