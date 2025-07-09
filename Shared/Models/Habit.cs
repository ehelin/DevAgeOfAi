using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class Habit
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Streak { get; set; }
        public string Category { get; set; } 
        public string Priority { get; set; } 
        public bool IsPaused { get; set; } = false;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public int SuccessCount { get; set; }
        public int TotalAttempts { get; set; }
        public string Status { get; set; }
    }
}
