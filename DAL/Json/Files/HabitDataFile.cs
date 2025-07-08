using Shared.Models;
using System.Text.Json;
using Shared;

namespace DAL.Json.Files
{
    public static class HabitDataFile
    {
        public static List<Habit> Load()
        {
            return JsonFileManager.Load<List<Habit>>(Constants.DataFile);
        }

        public static void Save(List<Habit> habits)
        {
            JsonFileManager.Save(Constants.DataFile, habits);
        }
    }
}