namespace Shared.Models
{
    public class StrategistState
    {
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public List<string> Suggestions { get; set; } = new();
    }
}
