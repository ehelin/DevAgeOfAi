using Shared.Models;
using System.Text.Json;
using Shared;

namespace DAL.Json.Files
{
    public static class HistorianAgentFile
    {
        public static List<HistorianState> Load()
        {
            return JsonFileManager.Load<List<HistorianState>>(Constants.HistorianStateFile);
        }

        public static void Save(List<HistorianState> historianStates)
        {
            JsonFileManager.Save(Constants.HistorianStateFile, historianStates);
        }
    }
}