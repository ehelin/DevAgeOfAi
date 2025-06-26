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
        private System.Windows.Forms.ListBox lstSuggestions;
        private System.Windows.Forms.ComboBox cmbSuggestedHabits;

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
            this.txtHabitName = new System.Windows.Forms.TextBox();
            this.btnAddHabit = new System.Windows.Forms.Button();
            this.lstHabits = new System.Windows.Forms.ListView();
            this.btnMarkCompleted = new System.Windows.Forms.Button();
            this.btnRemoveCompleted = new System.Windows.Forms.Button();
            this.cmbPriority = new System.Windows.Forms.ComboBox();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.lstSuggestions = new System.Windows.Forms.ListBox();
            this.cmbSuggestedHabits = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();

            // 
            // txtHabitName
            // 
            this.txtHabitName.Location = new System.Drawing.Point(12, 12);
            this.txtHabitName.Name = "txtHabitName";
            this.txtHabitName.Size = new System.Drawing.Size(200, 20);
            this.txtHabitName.TabIndex = 0;

            // 
            // btnAddHabit
            // 
            this.btnAddHabit.Location = new System.Drawing.Point(220, 10);
            this.btnAddHabit.Name = "btnAddHabit";
            this.btnAddHabit.Size = new System.Drawing.Size(75, 23);
            this.btnAddHabit.TabIndex = 1;
            this.btnAddHabit.Text = "Add Habit";
            this.btnAddHabit.UseVisualStyleBackColor = true;
            this.btnAddHabit.Click += new System.EventHandler(this.btnAddHabit_Click);

            // 
            // cmbPriority
            // 
            this.cmbPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPriority.FormattingEnabled = true;
            this.cmbPriority.Items.AddRange(new object[] {
            "Low",
            "Medium",
            "High"});
            this.cmbPriority.Location = new System.Drawing.Point(320, 10);
            this.cmbPriority.Name = "cmbPriority";
            this.cmbPriority.Size = new System.Drawing.Size(75, 23);
            this.cmbPriority.TabIndex = 5;

            // 
            // cmbCategory
            // 
            this.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategory.FormattingEnabled = true;
            this.cmbCategory.Items.AddRange(new object[] {
            "Health",
            "Work",
            "Personal Development",
            "Other"});
            this.cmbCategory.Location = new System.Drawing.Point(400, 10);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(120, 23);
            this.cmbCategory.TabIndex = 6;

            // 
            // lstHabits
            // 
            this.lstHabits.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            new System.Windows.Forms.ColumnHeader() { Text = "Habit", Width = 150 },
            new System.Windows.Forms.ColumnHeader() { Text = "Status", Width = 100 },
            new System.Windows.Forms.ColumnHeader() { Text = "Streak", Width = 80 },
            new System.Windows.Forms.ColumnHeader() { Text = "Priority", Width = 80 },
            new System.Windows.Forms.ColumnHeader() { Text = "Category", Width = 100 }});
            this.lstHabits.FullRowSelect = true;
            this.lstHabits.GridLines = true;
            this.lstHabits.Location = new System.Drawing.Point(12, 50);
            this.lstHabits.Name = "lstHabits";
            this.lstHabits.Size = new System.Drawing.Size(510, 250);
            this.lstHabits.TabIndex = 2;
            this.lstHabits.UseCompatibleStateImageBehavior = false;
            this.lstHabits.View = System.Windows.Forms.View.Details;

            // Anchor ListView to expand with the form
            this.lstHabits.Anchor = (System.Windows.Forms.AnchorStyles.Top |
                                     System.Windows.Forms.AnchorStyles.Bottom |
                                     System.Windows.Forms.AnchorStyles.Left |
                                     System.Windows.Forms.AnchorStyles.Right);

            // 
            // btnMarkCompleted
            // 
            this.btnMarkCompleted.Location = new System.Drawing.Point(12, 320);
            this.btnMarkCompleted.Name = "btnMarkCompleted";
            this.btnMarkCompleted.Size = new System.Drawing.Size(100, 23);
            this.btnMarkCompleted.TabIndex = 3;
            this.btnMarkCompleted.Text = "Mark Completed";
            this.btnMarkCompleted.UseVisualStyleBackColor = true;
            this.btnMarkCompleted.Click += new System.EventHandler(this.btnMarkCompleted_Click);

            // 
            // btnRemoveCompleted
            // 
            this.btnRemoveCompleted.Location = new System.Drawing.Point(195, 320);
            this.btnRemoveCompleted.Name = "btnRemoveCompleted";
            this.btnRemoveCompleted.Size = new System.Drawing.Size(150, 23);
            this.btnRemoveCompleted.TabIndex = 4;
            this.btnRemoveCompleted.Text = "Remove Completed";
            this.btnRemoveCompleted.UseVisualStyleBackColor = true;
            this.btnRemoveCompleted.Click += new System.EventHandler(this.btnRemoveCompleted_Click);

            this.btnGenerateHabit = new System.Windows.Forms.Button();
            this.btnGenerateHabit.Location = new System.Drawing.Point(372, 320); // Adjust position
            this.btnGenerateHabit.Name = "btnGenerateHabit";
            this.btnGenerateHabit.Size = new System.Drawing.Size(150, 23);
            this.btnGenerateHabit.Text = "Give me habits to track";
            this.btnGenerateHabit.UseVisualStyleBackColor = true;
            this.btnGenerateHabit.Click += new System.EventHandler(this.btnGenerateHabit_Click);


            this.lstSuggestions = new System.Windows.Forms.ListBox();
            this.lstSuggestions.FormattingEnabled = true;
            this.lstSuggestions.Location = new System.Drawing.Point(600, 30);
            this.lstSuggestions.Name = "lstSuggestions";
            this.lstSuggestions.Size = new System.Drawing.Size(300, 400);
            this.Controls.Add(this.lstSuggestions);

            this.cmbSuggestedHabits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSuggestedHabits.FormattingEnabled = true;
            this.cmbSuggestedHabits.Location = new System.Drawing.Point(600, 10); // Just above lstSuggestions
            this.cmbSuggestedHabits.Name = "cmbSuggestedHabits";
            this.cmbSuggestedHabits.Size = new System.Drawing.Size(300, 23);
            this.Controls.Add(this.cmbSuggestedHabits);

            // 
            // HabitTrackerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 400);  // Initial form size
            this.Controls.Add(this.cmbPriority);
            this.Controls.Add(this.cmbCategory);
            this.Controls.Add(this.lstHabits);
            this.Controls.Add(this.btnAddHabit);
            this.Controls.Add(this.txtHabitName);
            this.Controls.Add(this.btnMarkCompleted);
            this.Controls.Add(this.btnRemoveCompleted);
            this.Controls.Add(this.btnGenerateHabit);
            this.Name = "HabitTrackerForm";
            this.Text = "Habit Tracker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}
