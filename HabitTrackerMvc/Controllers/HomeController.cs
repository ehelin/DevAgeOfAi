using HabitTracker.Models;
using HabitTrackerMvc.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HabitTrackerMvc.Controllers
{
    public class HomeController : Controller

    {
        // Action to display the list of habits
        public IActionResult Index()
        {
            var habits = Habit.LoadHabits(); // This should be a List<Habit>
            return View(habits); // Pass the list of habits to the view
        }

        // Action to display the form for adding a new habit
        public IActionResult Create()
        {
            return View();
        }

        // Action to handle form submission
        [HttpPost]
        public IActionResult Create(Habit habit)
        {
            if (ModelState.IsValid)
            {
                var habits = Habit.LoadHabits();
                habits.Add(habit);
                Habit.SaveHabits(habits);
                return RedirectToAction("Index");
            }
            return View(habit);
        }
    }
}
