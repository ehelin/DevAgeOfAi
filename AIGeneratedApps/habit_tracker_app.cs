// Models/Habit.cs
using System;

namespace HabitTracker.Models
{
    public enum Priority
    {
        Low,
        Medium,
        High
    }

    public class Habit
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Priority Priority { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public Habit()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.Now;
            IsCompleted = false;
        }

        public Habit(string name, string description, Priority priority) : this()
        {
            Name = name;
            Description = description;
            Priority = priority;
        }
    }
}

// Data/IHabitRepository.cs
using System;
using System.Collections.Generic;
using HabitTracker.Models;

namespace HabitTracker.Data
{
    public interface IHabitRepository
    {
        List<Habit> GetAll();
        Habit GetById(Guid id);
        void Add(Habit habit);
        void Update(Habit habit);
        void Delete(Guid id);
        void SaveToFile();
        void LoadFromFile();
    }
}

// Data/FileHabitRepository.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using HabitTracker.Models;

namespace HabitTracker.Data
{
    public class FileHabitRepository : IHabitRepository
    {
        private readonly string _filePath;
        private List<Habit> _habits;

        public FileHabitRepository(string filePath = "habits.json")
        {
            _filePath = filePath;
            _habits = new List<Habit>();
            LoadFromFile();
        }

        public List<Habit> GetAll()
        {
            return _habits.ToList();
        }

        public Habit GetById(Guid id)
        {
            return _habits.FirstOrDefault(h => h.Id == id);
        }

        public void Add(Habit habit)
        {
            if (habit == null) throw new ArgumentNullException(nameof(habit));
            _habits.Add(habit);
            SaveToFile();
        }

        public void Update(Habit habit)
        {
            if (habit == null) throw new ArgumentNullException(nameof(habit));
            
            var existingHabit = GetById(habit.Id);
            if (existingHabit != null)
            {
                var index = _habits.IndexOf(existingHabit);
                _habits[index] = habit;
                SaveToFile();
            }
        }

        public void Delete(Guid id)
        {
            var habit = GetById(id);
            if (habit != null)
            {
                _habits.Remove(habit);
                SaveToFile();
            }
        }

        public void SaveToFile()
        {
            try
            {
                var json = JsonSerializer.Serialize(_habits, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save habits to file: {ex.Message}", ex);
            }
        }

        public void LoadFromFile()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        _habits = JsonSerializer.Deserialize<List<Habit>>(json) ?? new List<Habit>();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load habits from file: {ex.Message}", ex);
            }
        }
    }
}

// Business/IHabitService.cs
using System;
using System.Collections.Generic;
using HabitTracker.Models;

namespace HabitTracker.Business
{
    public interface IHabitService
    {
        List<Habit> GetAllHabits();
        Habit GetHabit(Guid id);
        void CreateHabit(string name, string description, Priority priority);
        void UpdateHabit(Guid id, string name, string description, Priority priority);
        void DeleteHabit(Guid id);
        void MarkHabitComplete(Guid id);
        void MarkHabitIncomplete(Guid id);
        List<Habit> GetHabitsByPriority(Priority priority);
        List<Habit> GetCompletedHabits();
        List<Habit> GetIncompleteHabits();
    }
}

// Business/HabitService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using HabitTracker.Data;
using HabitTracker.Models;

namespace HabitTracker.Business
{
    public class HabitService : IHabitService
    {
        private readonly IHabitRepository _habitRepository;

        public HabitService(IHabitRepository habitRepository)
        {
            _habitRepository = habitRepository ?? throw new ArgumentNullException(nameof(habitRepository));
        }

        public List<Habit> GetAllHabits()
        {
            return _habitRepository.GetAll();
        }

        public Habit GetHabit(Guid id)
        {
            return _habitRepository.GetById(id);
        }

        public void CreateHabit(string name, string description, Priority priority)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Habit name cannot be empty", nameof(name));

            var habit = new Habit(name.Trim(), description?.Trim() ?? string.Empty, priority);
            _habitRepository.Add(habit);
        }

        public void UpdateHabit(Guid id, string name, string description, Priority priority)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Habit name cannot be empty", nameof(name));

            var habit = _habitRepository.GetById(id);
            if (habit == null)
                throw new InvalidOperationException("Habit not found");

            habit.Name = name.Trim();
            habit.Description = description?.Trim() ?? string.Empty;
            habit.Priority = priority;

            _habitRepository.Update(habit);
        }

        public void DeleteHabit(Guid id)
        {
            _habitRepository.Delete(id);
        }

        public void MarkHabitComplete(Guid id)
        {
            var habit = _habitRepository.GetById(id);
            if (habit == null)
                throw new InvalidOperationException("Habit not found");

            if (!habit.IsCompleted)
            {
                habit.IsCompleted = true;
                habit.CompletedDate = DateTime.Now;
                _habitRepository.Update(habit);
            }
        }

        public void MarkHabitIncomplete(Guid id)
        {
            var habit = _habitRepository.GetById(id);
            if (habit == null)
                throw new InvalidOperationException("Habit not found");

            if (habit.IsCompleted)
            {
                habit.IsCompleted = false;
                habit.CompletedDate = null;
                _habitRepository.Update(habit);
            }
        }

        public List<Habit> GetHabitsByPriority(Priority priority)
        {
            return _habitRepository.GetAll().Where(h => h.Priority == priority).ToList();
        }

        public List<Habit> GetCompletedHabits()
        {
            return _habitRepository.GetAll().Where(h => h.IsCompleted).ToList();
        }

        public List<Habit> GetIncompleteHabits()
        {
            return _habitRepository.GetAll().Where(h => !h.IsCompleted).ToList();
        }
    }
}

// Forms/MainForm.cs
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HabitTracker.Business;
using HabitTracker.Models;

namespace HabitTracker.Forms
{
    public partial class MainForm : Form
    {
        private readonly IHabitService _habitService;
        private DataGridView dgvHabits;
        private Button btnAdd, btnEdit, btnDelete, btnToggleComplete;
        private ComboBox cbFilter;
        private Label lblFilter;

        public MainForm(IHabitService habitService)
        {
            _habitService = habitService ?? throw new ArgumentNullException(nameof(habitService));
            InitializeComponent();
            LoadHabits();
        }

        private void InitializeComponent()
        {
            this.Text = "Habit Tracker";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Filter controls
            lblFilter = new Label
            {
                Text = "Filter:",
                Location = new Point(10, 10),
                Size = new Size(50, 23)
            };

            cbFilter = new ComboBox
            {
                Location = new Point(70, 10),
                Size = new Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbFilter.Items.AddRange(new object[] { "All", "Completed", "Incomplete", "High Priority", "Medium Priority", "Low Priority" });
            cbFilter.SelectedIndex = 0;
            cbFilter.SelectedIndexChanged += CbFilter_SelectedIndexChanged;

            // DataGridView
            dgvHabits = new DataGridView
            {
                Location = new Point(10, 40),
                Size = new Size(760, 450),
                Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right),
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Buttons
            btnAdd = new Button
            {
                Text = "Add Habit",
                Location = new Point(10, 500),
                Size = new Size(100, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button
            {
                Text = "Edit Habit",
                Location = new Point(120, 500),
                Size = new Size(100, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button
            {
                Text = "Delete Habit",
                Location = new Point(230, 500),
                Size = new Size(100, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnDelete.Click += BtnDelete_Click;

            btnToggleComplete = new Button
            {
                Text = "Toggle Complete",
                Location = new Point(340, 500),
                Size = new Size(120, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnToggleComplete.Click += BtnToggleComplete_Click;

            // Add controls to form
            this.Controls.AddRange(new Control[] { lblFilter, cbFilter, dgvHabits, btnAdd, btnEdit, btnDelete, btnToggleComplete });
        }

        private void LoadHabits()
        {
            try
            {
                var habits = _habitService.GetAllHabits()
                    .Select(h => new
                    {
                        Id = h.Id,
                        Name = h.Name,
                        Description = h.Description,
                        Priority = h.Priority.ToString(),
                        Status = h.IsCompleted ? "Completed" : "Incomplete",
                        CreatedDate = h.CreatedDate.ToString("yyyy-MM-dd"),
                        CompletedDate = h.CompletedDate?.ToString("yyyy-MM-dd") ?? ""
                    }).ToList();

                dgvHabits.DataSource = habits;
                
                if (dgvHabits.Columns["Id"] != null)
                    dgvHabits.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading habits: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var habits = cbFilter.SelectedItem.ToString() switch
                {
                    "Completed" => _habitService.GetCompletedHabits(),
                    "Incomplete" => _habitService.GetIncompleteHabits(),
                    "High Priority" => _habitService.GetHabitsByPriority(Priority.High),
                    "Medium Priority" => _habitService.GetHabitsByPriority(Priority.Medium),
                    "Low Priority" => _habitService.GetHabitsByPriority(Priority.Low),
                    _ => _habitService.GetAllHabits()
                };

                var displayHabits = habits.Select(h => new
                {
                    Id = h.Id,
                    Name = h.Name,
                    Description = h.Description,
                    Priority = h.Priority.ToString(),
                    Status = h.IsCompleted ? "Completed" : "Incomplete",
                    CreatedDate = h.CreatedDate.ToString("yyyy-MM-dd"),
                    CompletedDate = h.CompletedDate?.ToString("yyyy-MM-dd") ?? ""
                }).ToList();

                dgvHabits.DataSource = displayHabits;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering habits: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new HabitForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _habitService.CreateHabit(form.HabitName, form.HabitDescription, form.HabitPriority);
                    LoadHabits();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding habit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (GetSelectedHabitId() is Guid selectedId)
            {
                var habit = _habitService.GetHabit(selectedId);
                if (habit != null)
                {
                    var form = new HabitForm(habit);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            _habitService.UpdateHabit(selectedId, form.HabitName, form.HabitDescription, form.HabitPriority);
                            LoadHabits();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error updating habit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a habit to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (GetSelectedHabitId() is Guid selectedId)
            {
                var result = MessageBox.Show("Are you sure you want to delete this habit?", "Confirm Delete", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        _habitService.DeleteHabit(selectedId);
                        LoadHabits();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting habit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a habit to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnToggleComplete_Click(object sender, EventArgs e)
        {
            if (GetSelectedHabitId() is Guid selectedId)
            {
                try
                {
                    var habit = _habitService.GetHabit(selectedId);
                    if (habit != null)
                    {
                        if (habit.IsCompleted)
                            _habitService.MarkHabitIncomplete(selectedId);
                        else
                            _habitService.MarkHabitComplete(selectedId);
                        
                        LoadHabits();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error toggling habit status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a habit to toggle completion status.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private Guid? GetSelectedHabitId()
        {
            if (dgvHabits.SelectedRows.Count > 0)
            {
                var row = dgvHabits.SelectedRows[0];
                if (row.DataBoundItem != null)
                {
                    var idProperty = row.DataBoundItem.GetType().GetProperty("Id");
                    if (idProperty != null && idProperty.GetValue(row.DataBoundItem) is Guid id)
                    {
                        return id;
                    }
                }
            }
            return null;
        }
    }
}

// Forms/HabitForm.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using HabitTracker.Models;

namespace HabitTracker.Forms
{
    public partial class HabitForm : Form
    {
        private TextBox txtName, txtDescription;
        private ComboBox cbPriority;
        private Button btnOK, btnCancel;
        private Label lblName, lblDescription, lblPriority;

        public string HabitName { get; private set; }
        public string HabitDescription { get; private set; }
        public Priority HabitPriority { get; private set; }

        public HabitForm(Habit habit = null)
        {
            InitializeComponent();
            
            if (habit != null)
            {
                txtName.Text = habit.Name;
                txtDescription.Text = habit.Description;
                cbPriority.SelectedItem = habit.Priority.ToString();
                this.Text = "Edit Habit";
            }
            else
            {
                this.Text = "Add Habit";
            }
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Name
            lblName = new Label
            {
                Text = "Name:",
                Location = new Point(10, 20),
                Size = new Size(80, 23)
            };

            txtName = new TextBox
            {
                Location = new Point(100, 20),
                Size = new Size(270, 23)
            };

            // Description
            lblDescription = new Label
            {
                Text = "Description:",
                Location = new Point(10, 60),
                Size = new Size(80, 23)
            };

            txtDescription = new TextBox
            {
                Location = new Point(100, 60),
                Size = new Size(270, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            // Priority
            lblPriority = new Label
            {
                Text = "Priority:",
                Location = new Point(10, 140),
                Size = new Size(80, 23)
            };

            cbPriority = new ComboBox
            {
                Location = new Point(100, 140),
                Size = new Size(100, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbPriority.Items.AddRange(new object[] { "Low", "Medium", "High" });
            cbPriority.SelectedIndex = 1; // Default to Medium

            // Buttons
            btnOK = new Button
            {
                Text = "OK",
                Location = new Point(210, 180),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK
            };
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(295, 180),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.AddRange(new Control[] { lblName, txtName, lblDescription, txtDescription, lblPriority, cbPriority, btnOK, btnCancel });
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a habit name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            HabitName = txtName.Text.Trim();
            HabitDescription = txtDescription.Text.Trim();
            HabitPriority = Enum.Parse<Priority>(cbPriority.SelectedItem.ToString());
        }
    }
}

// Program.cs
using System;
using System.Windows.Forms;
using HabitTracker.Business;
using HabitTracker.Data;
using HabitTracker.Forms;

namespace HabitTracker
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Setup dependency injection
                IHabitRepository habitRepository = new FileHabitRepository();
                IHabitService habitService = new HabitService(habitRepository);

                // Run the application
                Application.Run(new MainForm(habitService));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application failed to start: {ex.Message}", "Startup Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}