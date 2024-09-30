using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Models
{
    public class Habit
    {
        public string Name { get; set; }
        public string Priority { get; set; } // e.g., High, Medium, Low
        public string Category { get; set; } // e.g., Health, Work, Learning
        public DateTime Date { get; set; }

        // Path to the data file
        private static string dataFile = "data.json";

        // Method to load habits from the JSON file
        public static List<Habit> LoadHabits()
        {
            if (!File.Exists(dataFile))
                return new List<Habit>();

            var jsonData = File.ReadAllText(dataFile);
            return JsonSerializer.Deserialize<List<Habit>>(jsonData) ?? new List<Habit>();
        }

        // Method to save habits to the JSON file
        public static void SaveHabits(List<Habit> habits)
        {
            var jsonData = JsonSerializer.Serialize(habits);
            File.WriteAllText(dataFile, jsonData);
        }
    }
}
