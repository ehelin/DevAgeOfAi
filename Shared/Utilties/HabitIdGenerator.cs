using HabitTracker.Models;

namespace Shared.Utilties
{
    public static class HabitIdGenerator
    {
        private static int _currentId = 0;

        public static string GetNextId()
        {
            _currentId++;
            return _currentId.ToString();
        }

        public static void SeedFromExisting(IEnumerable<Habit> existingHabits)
        {
            if (existingHabits?.Any() == true)
            {
                _currentId = existingHabits
                    .Select(h => int.TryParse(h.Id, out var id) ? id : 0)
                    .DefaultIfEmpty(0)
                    .Max();
            }
        }
    }
}
