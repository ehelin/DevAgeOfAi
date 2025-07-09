namespace Shared.Models
{
    public class BuilderState
    {
        public DateTime LastRun { get; set; } = DateTime.UtcNow;

        public List<string> AppliedSuggestions { get; set; } = new();

        public List<string> SkippedSuggestions { get; set; } = new();
    }
}
