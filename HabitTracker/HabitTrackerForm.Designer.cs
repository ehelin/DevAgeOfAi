namespace HabitTracker
{
    partial class HabitTrackerForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtHabitName;
        private System.Windows.Forms.Button btnAddHabit;
        private System.Windows.Forms.ListView lstHabits;
        private System.Windows.Forms.Button btnMarkCompleted;
        private System.Windows.Forms.Button btnRemoveCompleted;
        private System.Windows.Forms.ComboBox cmbPriority;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Button btnGenerateHabit;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtHabitName = new TextBox();
            btnAddHabit = new Button();
            lstHabits = new ListView();
            btnMarkCompleted = new Button();
            btnRemoveCompleted = new Button();
            cmbPriority = new ComboBox();
            cmbCategory = new ComboBox();
            btnGenerateHabit = new Button();
            SuspendLayout();
            // 
            // txtHabitName
            // 
            txtHabitName.Location = new Point(16, 18);
            txtHabitName.Margin = new Padding(4, 5, 4, 5);
            txtHabitName.Name = "txtHabitName";
            txtHabitName.Size = new Size(265, 27);
            txtHabitName.TabIndex = 0;
            // 
            // btnAddHabit
            // 
            btnAddHabit.Location = new Point(293, 15);
            btnAddHabit.Margin = new Padding(4, 5, 4, 5);
            btnAddHabit.Name = "btnAddHabit";
            btnAddHabit.Size = new Size(100, 35);
            btnAddHabit.TabIndex = 1;
            btnAddHabit.Text = "Add Habit";
            btnAddHabit.UseVisualStyleBackColor = true;
            btnAddHabit.Click += btnAddHabit_Click;
            // 
            // lstHabits
            // 
            lstHabits.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstHabits.FullRowSelect = true;
            lstHabits.GridLines = true;
            lstHabits.Location = new Point(16, 77);
            lstHabits.Margin = new Padding(4, 5, 4, 5);
            lstHabits.Name = "lstHabits";
            lstHabits.Size = new Size(679, 382);
            lstHabits.TabIndex = 2;
            lstHabits.UseCompatibleStateImageBehavior = false;
            lstHabits.View = View.Details;
            // 
            // btnMarkCompleted
            // 
            btnMarkCompleted.Location = new Point(16, 492);
            btnMarkCompleted.Margin = new Padding(4, 5, 4, 5);
            btnMarkCompleted.Name = "btnMarkCompleted";
            btnMarkCompleted.Size = new Size(133, 35);
            btnMarkCompleted.TabIndex = 3;
            btnMarkCompleted.Text = "Mark Completed";
            btnMarkCompleted.UseVisualStyleBackColor = true;
            btnMarkCompleted.Click += btnMarkCompleted_Click;
            // 
            // btnRemoveCompleted
            // 
            btnRemoveCompleted.Location = new Point(260, 492);
            btnRemoveCompleted.Margin = new Padding(4, 5, 4, 5);
            btnRemoveCompleted.Name = "btnRemoveCompleted";
            btnRemoveCompleted.Size = new Size(133, 35);
            btnRemoveCompleted.TabIndex = 4;
            btnRemoveCompleted.Text = "Remove Completed";
            btnRemoveCompleted.UseVisualStyleBackColor = true;
            btnRemoveCompleted.Click += btnRemoveCompleted_Click;
            // 
            // cmbPriority
            // 
            cmbPriority.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPriority.FormattingEnabled = true;
            cmbPriority.Items.AddRange(new object[] { "Low", "Medium", "High" });
            cmbPriority.Location = new Point(427, 15);
            cmbPriority.Margin = new Padding(4, 5, 4, 5);
            cmbPriority.Name = "cmbPriority";
            cmbPriority.Size = new Size(99, 28);
            cmbPriority.TabIndex = 5;
            // 
            // cmbCategory
            // 
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategory.FormattingEnabled = true;
            cmbCategory.Items.AddRange(new object[] { "Health", "Work", "Personal Development", "Other" });
            cmbCategory.Location = new Point(533, 15);
            cmbCategory.Margin = new Padding(4, 5, 4, 5);
            cmbCategory.Name = "cmbCategory";
            cmbCategory.Size = new Size(159, 28);
            cmbCategory.TabIndex = 6;
            // 
            // btnGenerateHabit
            // 
            btnGenerateHabit.Location = new Point(515, 492);
            btnGenerateHabit.Margin = new Padding(4, 5, 4, 5);
            btnGenerateHabit.Name = "btnGenerateHabit";
            btnGenerateHabit.Size = new Size(180, 35);
            btnGenerateHabit.TabIndex = 7;
            btnGenerateHabit.Text = "Give me habits to track";
            btnGenerateHabit.UseVisualStyleBackColor = true;
            btnGenerateHabit.Click += btnGenerateHabit_Click;
            // 
            // HabitTrackerForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 615);
            Controls.Add(cmbPriority);
            Controls.Add(cmbCategory);
            Controls.Add(lstHabits);
            Controls.Add(btnAddHabit);
            Controls.Add(txtHabitName);
            Controls.Add(btnMarkCompleted);
            Controls.Add(btnRemoveCompleted);
            Controls.Add(btnGenerateHabit);
            Margin = new Padding(4, 5, 4, 5);
            Name = "HabitTrackerForm";
            Text = "Habit Tracker";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
