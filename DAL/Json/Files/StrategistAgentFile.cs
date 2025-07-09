using Shared.Models;
using System.Text.Json;
using Shared;

namespace DAL.Json.Files
{
    public static class StrategistAgentFile
    {
        public static StrategistState Load()
        {
            return JsonFileManager.Load<StrategistState>(Constants.StrategistStateFile);
        }

        public static void Save(StrategistState strategistState)
        {
            JsonFileManager.Save(Constants.StrategistStateFile, strategistState);
        }
    }
}