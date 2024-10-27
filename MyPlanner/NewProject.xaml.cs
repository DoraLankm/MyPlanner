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

            // Установим значения по умолчанию
            cbPriority.SelectedIndex = 0;
            cbCategory.SelectedIndex = 0;
            dpDeadline.SelectedDate = DateTime.Now.AddDays(7);
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProjectName.Text))
            {
                MessageBox.Show("Пожалуйста, введите название проекта.");
                return;
            }

            // Создаем новый проект
            Project = new Project
            {
                Name = txtProjectName.Text,
                Description = txtProjectDescription.Text,
                Deadline = dpDeadline.SelectedDate ?? DateTime.Now.AddDays(7),
                Priority = (Priority)Enum.Parse(typeof(Priority), ((ComboBoxItem)cbPriority.SelectedItem).Content.ToString()),
                Category = (Category)Enum.Parse(typeof(Category), ((ComboBoxItem)cbCategory.SelectedItem).Content.ToString()),
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
