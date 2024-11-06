using MyPlanner.Models;
using System;
using System.Windows;
using TaskStatus = MyPlanner.Models.TaskStatus;

namespace MyPlanner
{
    public partial class TaskWindow : Window
    {
        public TaskClass NewTask { get; private set; }
        public Project Project { get; private set; }
        bool isEditing;

        public TaskWindow(Project selected_project)
        {
            InitializeComponent();
            dpTaskDeadline.SelectedDate = DateTime.UtcNow.AddDays(7);
            Project = selected_project;
            isEditing = false;
            cbTaskStatus.Visibility = Visibility.Hidden;
            labelTaskStatus.Visibility = Visibility.Hidden;
        }

        public TaskWindow(Project selected_project, TaskClass selected_task)
        {
            InitializeComponent();
            Project = selected_project;
            NewTask = selected_task;
            isEditing = true;
            cbTaskStatus.Visibility = Visibility.Visible;
            txtTaskTitle.Text = NewTask.Title;
            txtTaskDescription.Text = NewTask.Description;
            dpTaskDeadline.SelectedDate = NewTask.Deadline.ToLocalTime();
            cbTaskStatus.SelectedItem = NewTask.Status;
        }

        bool IsTaskNameUnique(string name)
        {
            foreach (var task in Project.Tasks)
            {
                if (task.Title.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    if (isEditing && name == NewTask.Title)
                    {
                        continue;
                    }
                    return false;
                }
            }

            return true;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTaskTitle.Text))
            {
                MessageBox.Show("Название задачи не может быть пустым!");
                return;
            }
            if (!IsTaskNameUnique(txtTaskTitle.Text))
            {
                MessageBox.Show("Задача уже существует!");
                return;
            }

            DateTime deadline = (dpTaskDeadline.SelectedDate ?? DateTime.UtcNow.AddDays(7)).ToUniversalTime();

            if (!isEditing)
            {
                NewTask = new TaskClass
                {
                    Title = txtTaskTitle.Text,
                    Description = txtTaskDescription.Text,
                    Deadline = deadline,
                    CreationDate = DateTime.UtcNow,
                    Status = TaskStatus.NotStarted,
                    ProjectId = Project.Id
                };
            }
            else
            {
                NewTask.Title = txtTaskTitle.Text;
                NewTask.Description = txtTaskDescription.Text;
                NewTask.Deadline = deadline;
                NewTask.Status = (TaskStatus)cbTaskStatus.SelectedItem;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
