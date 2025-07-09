using Shared.Models;
using Shared;

namespace DAL.Json.Files
{
    public static class BuilderAgentFile
    {
        public static BuilderState Load()
        {
            return JsonFileManager.Load<BuilderState>(Constants.BuilderStateFile);
        }

        public static void Save(BuilderState builderState)
        {
            JsonFileManager.Save(Constants.BuilderStateFile, builderState);
        }
    }
}
