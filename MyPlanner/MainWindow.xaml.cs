using MyPlanner.Models;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskStatus = MyPlanner.Models.TaskStatus;

namespace MyPlanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ObservableCollection<Project> Projects = new ObservableCollection<Project>();
        private ObservableCollection<TaskClass> Tasks = new ObservableCollection<TaskClass>();
        private Project selectedProject;

        private void AddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            string currentDate = DateTime.Now.ToString(); // текущая дата и время

            // Создаем TextBox для ввода текста новой заметки
            TextBox noteTextBox = new TextBox
            {
                Style = (Style)FindResource("NoteTextBoxStyle")
            };

            noteTextBox.TextChanged += (s, ev) =>
            {
                if (noteTextBox.Text.Length > 250)
                {
                    noteTextBox.Text = noteTextBox.Text.Substring(0, 250);
                    noteTextBox.CaretIndex = noteTextBox.Text.Length; // Ставим курсор в конец
                }
            };

            // Оформление пузырька заметки
            Border noteBubble = new Border
            {
                Style = (Style)FindResource("ChatBubbleStyle"),
                Child = new StackPanel
                {
                    Children =
                    {
                        noteTextBox,
                        new TextBlock
                        {
                            Style = (Style)FindResource("DateTextBlockStyle"),
                            Text = currentDate
                        }
                    }
                }
            };

            NotesWrapPanel.Children.Add(noteBubble);
            noteTextBox.Focus();
            noteTextBox.LostFocus += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(noteTextBox.Text))
                {
                    // Если текст пустой или состоит только из пробелов, удаляем заметку
                    NotesWrapPanel.Children.Remove(noteBubble);
                }
            };
        }



        private void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            NewProject newProjectWindow = new NewProject();
            if (newProjectWindow.ShowDialog() == true)
            {
                Project newProject = newProjectWindow.Project;

                // Создаем контейнер для проекта с использованием ваших предпочтений
                Border projectBorder = CreateProjectBorder(newProject);

                // Добавляем проект в WrapPanel
                ProjectsPanel.Children.Add(projectBorder);

                // Добавляем проект в коллекцию проектов (если используется)
                Projects.Add(newProject);
            }
        }

        private Border CreateProjectBorder(Project project)
        {
            // Создаем контейнер для проекта
            Border projectBorder = new Border
            {
                Width = 150,
                Height = 150,
                Margin = new Thickness(5),
                BorderBrush = (Brush)this.Resources["Color1"],
                BorderThickness = new Thickness(1),
                Background = (Brush)this.Resources["Color4"],
                Cursor = Cursors.Hand,
                Tag = project // Связываем объект проекта с квадратом
            };

            // StackPanel для текстовых блоков проекта
            StackPanel projectContent = new StackPanel();

            TextBlock projectName = new TextBlock
            {
                Text = project.Name,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5)
            };

            TextBlock projectPriority = new TextBlock
            {
                Text = $"Приоритет: {project.Priority}",
                Margin = new Thickness(5)
            };

            TextBlock projectCategory = new TextBlock
            {
                Text = $"Категория: {project.Category}",
                Margin = new Thickness(5)
            };

            TextBlock projectCreationDate = new TextBlock
            {
                Text = $"Создан: {project.CreationDate:dd.MM.yyyy}",
                Margin = new Thickness(5)
            };

            TextBlock projectDeadline = new TextBlock
            {
                Text = $"Дедлайн: {project.Deadline:dd.MM.yyyy}",
                Margin = new Thickness(5)
            };

            // Добавляем текстовые блоки в StackPanel
            projectContent.Children.Add(projectName);
            projectContent.Children.Add(projectPriority);
            projectContent.Children.Add(projectCategory);
            projectContent.Children.Add(projectCreationDate);
            projectContent.Children.Add(projectDeadline);

            // Добавляем StackPanel в Border
            projectBorder.Child = projectContent;

            // Добавляем обработчик события для выделения проекта
            projectBorder.MouseLeftButtonUp += ProjectBorder_MouseLeftButtonUp;

            return projectBorder;
        }


        private void ProjectBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var selectedBorder = sender as Border;
            var selectedProject = selectedBorder.Tag as Project;

            // Сохраняем выбранный проект
            this.selectedProject = selectedProject;

            // Выделяем выбранный проект
            HighlightSelectedProject(selectedBorder);

            // Отображаем детали проекта
            DisplayProjectDetails(selectedProject);

            // Загружаем задачи проекта
            LoadTasks(selectedProject.Id);
        }

        private void HighlightSelectedProject(Border selectedBorder)
        {
            // Сброс выделения со всех проектов
            foreach (var child in ProjectsPanel.Children)
            {
                if (child is Border border)
                {
                    border.Background = (Brush)this.Resources["Color4"]; // Стандартный фон
                }
            }

            // Выделяем выбранный проект
            selectedBorder.Background = (Brush)this.Resources["Color3"];
        }

        private void DisplayProjectDetails(Project project)
        {
            // Отображаем название проекта
            txtProjectTitle.Text = project.Name;

            // Отображаем описание проекта
            txtProjectDescription.Text = project.Description;
        }

        private void RefreshTasksDisplay()
        {
            TasksDataGrid.ItemsSource = null;
            TasksDataGrid.ItemsSource = Tasks;
        }


        private void LoadTasks(int projectId)
        {
            // Очищаем список задач
            Tasks.Clear();

            // Загрузка задач, связанных с projectId
            // Если не используете базу данных, можно добавить тестовые задачи
            // Например:
            if (projectId == 1) // Замените на реальные условия
            {
                Tasks.Add(new TaskClass { Title = "Задача 1", Description = "Описание задачи 1", CreationDate = DateTime.Now, Deadline = DateTime.Now.AddDays(5), Status = TaskStatus.NotStarted });
                Tasks.Add(new TaskClass { Title = "Задача 2", Description = "Описание задачи 2", CreationDate = DateTime.Now, Deadline = DateTime.Now.AddDays(10), Status = TaskStatus.InProgress });
            }

            // Обновляем отображение задач
            RefreshTasksDisplay();
        }


        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            //автоматически добавлять строки в таблицу?
        }

        private void TasksDataGrid_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        

    }


}