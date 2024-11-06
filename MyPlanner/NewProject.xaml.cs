using MyPlanner.Models;
using MyPlanner.Services;
using System;
using System.Linq;
using System.Windows;

namespace MyPlanner
{
    public partial class NewProject : Window
    {
        User User { get; set; }
        public Project Project { get; private set; }
        private readonly ProjectService _projectService;
        bool isEditing;

        public NewProject(User user, ProjectService projectService)
        {
            InitializeComponent();

            cbPriority.SelectedItem = Priority.Medium;
            cbCategory.SelectedItem = Category.None;
            dpDeadline.SelectedDate = DateTime.UtcNow.AddDays(7);
            isEditing = false;
            User = user;
            _projectService = projectService;
        }

        public NewProject(User user, Project project, ProjectService projectService)
        {
            InitializeComponent();
            isEditing = true;
            Project = project;
            cbCategory.SelectedItem = Project.Category;
            cbPriority.SelectedItem = Project.Priority;
            txtProjectName.Text = Project.Name;
            txtProjectDescription.Text = Project.Description;
            dpDeadline.SelectedDate = Project.Deadline.ToLocalTime();
            User = user;
            _projectService = projectService;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProjectName.Text))
            {
                MessageBox.Show("Название проекта не может быть пустым!");
                txtProjectName.Focus();
                return;
            }
            if (!IsProjectNameUnique(txtProjectName.Text))
            {
                MessageBox.Show("Проект с таким названием уже существует!");
                return;
            }

            DateTime deadline = (dpDeadline.SelectedDate ?? DateTime.UtcNow.AddDays(7)).ToUniversalTime();

            if (!isEditing)
            {
                Project = new Project
                {
                    Name = txtProjectName.Text,
                    Description = txtProjectDescription.Text,
                    Deadline = deadline,
                    Priority = (Priority)cbPriority.SelectedItem,
                    Category = (Category)cbCategory.SelectedItem,
                    CreationDate = DateTime.UtcNow,
                    IsCompleted = false,
                    UserId = User.Id
                };
            }
            else
            {
                Project.Name = txtProjectName.Text;
                Project.Description = txtProjectDescription.Text;
                Project.Category = (Category)cbCategory.SelectedItem;
                Project.Priority = (Priority)cbPriority.SelectedItem;
                Project.Deadline = deadline;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        bool IsProjectNameUnique(string name)
        {
            var existingProject = _projectService.GetProjectByNameAndUserId(name, User.Id);

            if (existingProject != null)
            {
                if (isEditing && existingProject.Id == Project.Id)
                {
                    return true;
                }
                return false;
            }

            return true;
        }
    }
}
