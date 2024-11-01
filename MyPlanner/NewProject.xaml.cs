using MyPlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyPlanner
{
    /// <summary>
    /// Логика взаимодействия для NewProject.xaml
    /// </summary>
    public partial class NewProject : Window
    {
        User User { get; set; }
        public Project Project { get; private set; }
        bool isEditing; //режим редактирования

        public NewProject(User user)
        {
            InitializeComponent();

            cbPriority.SelectedItem = Priority.Medium; // Значение по умолчанию
            cbCategory.SelectedItem = Category.None; // Значение по умолчанию
            dpDeadline.SelectedDate = DateTime.Now.AddDays(7); //Значение по умолчанию
            isEditing = false; //режим создания нового проекта
            User = user;
        }

        public NewProject(User user,Project project)
        {
            InitializeComponent();
            isEditing = true; //режим редактирования существующего проекта 
            Project = project;
            cbCategory.SelectedItem = Project.Category;
            cbPriority.SelectedItem = Project.Priority;
            txtProjectName.Text = Project.Name;
            txtProjectDescription.Text = Project.Description;
            dpDeadline.SelectedDate = Project.Deadline;
            User = user;

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

            if (!isEditing) //создание новой задачи
            {
                // Создаем новый проект
                Project = new Project
                {
                    Name = txtProjectName.Text,
                    Description = txtProjectDescription.Text,
                    Deadline = dpDeadline.SelectedDate ?? DateTime.Now.AddDays(7),
                    Priority = (Priority)cbPriority.SelectedItem,
                    Category = (Category)cbCategory.SelectedItem,
                    CreationDate = DateTime.Now,
                    IsCompleted = false,
                    User = User
                };
            }
            else
            {
                Project.Name = txtProjectName.Text;
                Project.Description = txtProjectDescription.Text;
                Project.Category = (Category)cbCategory.SelectedItem;
                Project.Priority = (Priority)cbPriority.SelectedItem;
                Project.Deadline = dpDeadline.SelectedDate ?? DateTime.Now.AddDays(7);
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
            foreach (var project in User.Projects)
            {
                if (project.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    if (isEditing && name == Project.Name) //режим редактирования,назвнаие не изменилось
                    {
                        continue;
                    }
                    return false;
                }
            }

            return true;
        }
    }
}
