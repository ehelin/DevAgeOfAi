using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Models
{
    public class Habit
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public int Streak { get; set; }
        public string Priority { get; set; } // New property for Priority
        public string Category { get; set; } // New property for Category


        public string Id { get; set; }  // TODO - add seed generator

        public int SuccessCount { get; set; }
        public int TotalAttempts { get; set; }
    }
}
