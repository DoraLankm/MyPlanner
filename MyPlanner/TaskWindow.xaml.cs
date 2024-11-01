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
using TaskStatus = MyPlanner.Models.TaskStatus;

namespace MyPlanner
{
    /// <summary>
    /// Логика взаимодействия для TaskWindow.xaml
    /// </summary>
    public partial class TaskWindow : Window
    {
        public TaskClass NewTask { get; private set; }
        public Project Project { get; private set; }
        bool isEditing;
        public TaskWindow(Project selected_project)
        {
            InitializeComponent();
            dpTaskDeadline.SelectedDate = DateTime.Now.AddDays(7);
            Project = selected_project;
            isEditing = false;
            cbTaskStatus.Visibility = Visibility.Hidden;
        }

        public TaskWindow(Project selected_project,TaskClass selected_task)
        {
            InitializeComponent();
            Project = selected_project;
            NewTask = selected_task;
            isEditing = true; //редактировнаие существующей задачи
            cbTaskStatus.Visibility = Visibility.Visible;
            txtTaskTitle.Text = NewTask.Title;
            txtTaskDescription.Text = NewTask.Description;
            dpTaskDeadline.SelectedDate = NewTask.Deadline;
            cbTaskStatus.SelectedItem = NewTask.Status;
        }

        bool IsTaskNameUnique(string name)
        {
            foreach (var task in Project.Tasks)
            {
                if (task.Title.Equals(name, StringComparison.OrdinalIgnoreCase)) 
                {
                    if (isEditing && name == NewTask.Title) //режим редактирования,назвнаие не изменилось
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

            if (!isEditing) //создание новой задачи
            {
                // Создаем новую задачу
                NewTask = new TaskClass
                {
                    Title = txtTaskTitle.Text,
                    Description = txtTaskDescription.Text,
                    Deadline = dpTaskDeadline.SelectedDate ?? DateTime.Now.AddDays(7),
                    CreationDate = DateTime.Now,
                    Status = TaskStatus.NotStarted,
                    ProjectId = Project.Id

                };
            }
            else //редактирование существующей
            {
                NewTask.Title = txtTaskTitle.Text;
                NewTask.Description = txtTaskDescription.Text;
                NewTask.Deadline = dpTaskDeadline.SelectedDate ?? DateTime.Now.AddDays(7);
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
