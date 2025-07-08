using Shared.Models;
using System.Text.Json;
using Shared;

namespace DAL.Json.Files
{
    public static class SuggestedDataFile
    {
        public static List<SuggestedHabit> Load()
        {
            return JsonFileManager.Load<List<SuggestedHabit>>(Constants.SuggestionsFile);
        }

        public static void Save(List<SuggestedHabit> habits)
        {
            JsonFileManager.Save(Constants.SuggestionsFile, habits);
        }
    }
}