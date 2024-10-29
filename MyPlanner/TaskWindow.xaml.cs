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
        public TaskWindow()
        {
            InitializeComponent();
            dpTaskDeadline.SelectedDate = DateTime.Now.AddDays(7);
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTaskTitle.Text))
            {
                MessageBox.Show("Введите название задачи.");
                return;
            }

            // Создаем новую задачу
            NewTask = new TaskClass
            {
                Title = txtTaskTitle.Text,
                Description = txtTaskDescription.Text,
                Deadline = dpTaskDeadline.SelectedDate ?? DateTime.Now.AddDays(7),
                CreationDate = DateTime.Now,
                Status = TaskStatus.NotStarted
            };

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
