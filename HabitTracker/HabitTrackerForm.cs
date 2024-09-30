using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using HabitTracker.Models;
using Newtonsoft.Json;

namespace HabitTracker
{
    public partial class HabitTrackerForm : Form
    {
        private const string FileName = "data.json";

        public HabitTrackerForm()
        {
            InitializeComponent();
            LoadHabitsFromFile(); // Load habits on start
        }

        private void btnAddHabit_Click(object sender, EventArgs e)
        {
            string habitName = txtHabitName.Text;
            string priority = cmbPriority.SelectedItem?.ToString() ?? "Low"; // Default to Low if not selected
            string category = cmbCategory.SelectedItem?.ToString() ?? "Other"; // Default to Other if not selected

            if (!string.IsNullOrWhiteSpace(habitName))
            {
                // Add habit to the list
                ListViewItem item = new ListViewItem(habitName);
                item.SubItems.Add("Not Completed");
                item.SubItems.Add("0"); // Initialize streak to 0
                item.SubItems.Add(priority); // Add Priority
                item.SubItems.Add(category); // Add Category
                lstHabits.Items.Add(item);

                // Clear input fields
                txtHabitName.Clear();
                cmbPriority.SelectedIndex = -1;
                cmbCategory.SelectedIndex = -1;

                // Save habits to file
                SaveHabitsToFile();
            }
            else
            {
                MessageBox.Show("Please enter a habit name.");
            }
        }

        private void btnMarkCompleted_Click(object sender, EventArgs e)
        {
            if (lstHabits.SelectedItems.Count > 0)
            {
                foreach (ListViewItem selectedItem in lstHabits.SelectedItems)
                {
                    // Mark habit as completed and increase streak
                    selectedItem.SubItems[1].Text = "Completed";

                    int streak = int.Parse(selectedItem.SubItems[2].Text);
                    streak++;
                    selectedItem.SubItems[2].Text = streak.ToString();
                }

                // Save updated habits to file
                SaveHabitsToFile();
            }
            else
            {
                MessageBox.Show("Please select a habit to mark as completed.");
            }
        }

        private void btnRemoveCompleted_Click(object sender, EventArgs e)
        {
            for (int i = lstHabits.Items.Count - 1; i >= 0; i--)
            {
                if (lstHabits.Items[i].SubItems[1].Text == "Completed")
                {
                    lstHabits.Items.RemoveAt(i); // Remove from list
                }
            }

            // Save updated list after removing completed habits
            SaveHabitsToFile();
        }

        private void SaveHabitsToFile()
        {
            var habits = new List<Habit>();

            foreach (ListViewItem item in lstHabits.Items)
            {
                var habit = new Habit
                {
                    Name = item.SubItems[0].Text,
                    Status = item.SubItems[1].Text,
                    Streak = int.Parse(item.SubItems[2].Text),
                    Priority = item.SubItems[3].Text, // Save Priority
                    Category = item.SubItems[4].Text  // Save Category
                };
                habits.Add(habit);
            }

            // Serialize the habits to JSON and save to file
            File.WriteAllText(FileName, JsonConvert.SerializeObject(habits, Formatting.Indented));
        }

        private void LoadHabitsFromFile()
        {
            // If file doesn't exist, create it
            if (!File.Exists(FileName))
            {
                File.Create(FileName).Close();
                return; // No data to load yet
            }

            // Load habits from the JSON file
            string json = File.ReadAllText(FileName);
            if (!string.IsNullOrEmpty(json))
            {
                var habits = JsonConvert.DeserializeObject<List<Habit>>(json);
                if (habits != null)
                {
                    foreach (var habit in habits)
                    {
                        ListViewItem item = new ListViewItem(habit.Name);
                        item.SubItems.Add(habit.Status);
                        item.SubItems.Add(habit.Streak.ToString());
                        item.SubItems.Add(habit.Priority); // Load Priority
                        item.SubItems.Add(habit.Category); // Load Category
                        lstHabits.Items.Add(item);
                    }
                }
            }
        }
    }
}
