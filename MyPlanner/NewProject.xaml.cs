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
        public Project Project { get; private set; }

        public NewProject()
        {
            InitializeComponent();

            // Заполняем ComboBox для приоритета
            cbPriority.ItemsSource = Enum.GetValues(typeof(Priority)).Cast<Priority>();
            cbPriority.SelectedItem = Priority.Medium; // Значение по умолчанию

            // Заполняем ComboBox для категории
            cbCategory.ItemsSource = Enum.GetValues(typeof(Category)).Cast<Category>();
            cbCategory.SelectedItem = Category.None; // Значение по умолчанию

            // Устанавливаем дату дедлайна по умолчанию
            dpDeadline.SelectedDate = DateTime.Now.AddDays(7);
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProjectName.Text))
            {
                MessageBox.Show("Пожалуйста, введите название проекта.");
                txtProjectName.Focus();
                return;
            }

            // Создаем новый проект
            Project = new Project
            {
                Name = txtProjectName.Text,
                Description = txtProjectDescription.Text,
                Deadline = dpDeadline.SelectedDate ?? DateTime.Now.AddDays(7),
                Priority = (Priority)cbPriority.SelectedItem,
                Category = (Category)cbCategory.SelectedItem,
                CreationDate = DateTime.Now,
                IsCompleted = false
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
