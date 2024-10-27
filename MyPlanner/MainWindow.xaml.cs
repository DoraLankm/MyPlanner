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

namespace MyPlanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Project> Projects;
        private Project selectedProject;
        private ObservableCollection<Note> Notes;
        private TaskClass selectedTask;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var createProjectWindow = new NewProject();
            if (createProjectWindow.ShowDialog() == true)
            {
                var newProject = createProjectWindow.Project;

                // Сохраняем в базу данных
                using (var context = new PlanerDbContext())
                {
                    // Предположим, что у нас есть текущий пользователь с Id = 1
                    newProject.UserId = 1;
                    context.Projects.Add(newProject);
                    context.SaveChanges();
                }

                // Обновляем список проектов
                Projects.Add(newProject);
                RefreshProjectsDisplay();
            }
        }

        private void LoadProjects()
        {
            using (var context = new PlanerDbContext())
            {
                // Загружаем проекты текущего пользователя (Id = 1)
                Projects = new ObservableCollection<Project>(context.Projects.Where(p => p.UserId == 1).ToList());
            }

            RefreshProjectsDisplay();
        }

        private void RefreshProjectsDisplay()
        {
            // Здесь вы обновляете UI для отображения проектов
            // Например, очищаете контейнер и добавляете новые элементы

            // Предположим, что у вас есть WrapPanel для проектов
            // Найдите его по имени в XAML и добавьте элементы
            var projectsPanel = FindName("ProjectsPanel") as WrapPanel;
            projectsPanel.Children.Clear();

            foreach (var project in Projects)
            {
                var projectBorder = CreateProjectBorder(project);
                projectsPanel.Children.Add(projectBorder);
            }
        }

        private Border CreateProjectBorder(Project project)
        {
            var border = new Border
            {
                Width = 150,
                Height = 150,
                Margin = new Thickness(5),
                BorderBrush = (Brush)FindResource("Color1"),
                BorderThickness = new Thickness(1),
                Background = (Brush)FindResource("Color4"),
                Cursor = Cursors.Hand,
                Tag = project // Связываем проект с элементом UI
            };

            border.MouseLeftButtonUp += ProjectBorder_MouseLeftButtonUp;

            var stackPanel = new StackPanel();

            stackPanel.Children.Add(new TextBlock
            {
                Text = project.Name,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5)
            });

            stackPanel.Children.Add(new TextBlock { Text = $"Приоритет: {project.Priority}", Margin = new Thickness(5) });
            stackPanel.Children.Add(new TextBlock { Text = $"Категория: {project.Category}", Margin = new Thickness(5) });
            stackPanel.Children.Add(new TextBlock { Text = $"Создан: {project.CreationDate.ToShortDateString()}", Margin = new Thickness(5) });
            stackPanel.Children.Add(new TextBlock { Text = $"Дедлайн: {project.Deadline.ToShortDateString()}", Margin = new Thickness(5) });

            border.Child = stackPanel;

            return border;
        }

        private void ProjectBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var selectedBorder = sender as Border;
            var selectedProject = selectedBorder.Tag as Project;

            // Обновляем выделение
            HighlightSelectedProject(selectedBorder);

            // Отображаем описание проекта и загружаем задачи
            DisplayProjectDetails(selectedProject);
            LoadTasks(selectedProject.Id);
        }

        private void HighlightSelectedProject(Border selectedBorder)
        {
            // Сброс выделения со всех проектов
            var projectsPanel = FindName("ProjectsPanel") as WrapPanel;
            foreach (Border border in projectsPanel.Children)
            {
                border.Background = (Brush)FindResource("Color4");
            }

            // Выделяем выбранный проект
            selectedBorder.Background = Brushes.Yellow;
        }

        private void DisplayProjectDetails(Project project)
        {
            // Отображаем название и описание проекта
            txtProjectTitle.Text = project.Name;
            txtProjectDescription.Text = project.Description;
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProject == null)
            {
                MessageBox.Show("Пожалуйста, выберите проект.");
                return;
            }

            var createTaskWindow = new CreateTaskWindow();
            if (createTaskWindow.ShowDialog() == true)
            {
                var newTask = createTaskWindow.NewTask;
                newTask.ProjectId = selectedProject.Id;

                // Сохраняем в базу данных
                using (var context = new PlanerDbContext())
                {
                    context.Tasks.Add(newTask);
                    context.SaveChanges();
                }

                // Обновляем список задач
                Tasks.Add(newTask);
                RefreshTasksDisplay();
            }
        }

        private void LoadTasks(int projectId)
        {
            using (var context = new PlanerDbContext())
            {
                Tasks = new ObservableCollection<TaskClass>(context.Tasks.Where(t => t.ProjectId == projectId).ToList());
            }

            RefreshTasksDisplay();
        }

        private void RefreshTasksDisplay()
        {
            // Обновляем DataGrid с задачами
            TasksDataGrid.ItemsSource = Tasks;
        }

        private void AddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTask == null)
            {
                MessageBox.Show("Пожалуйста, выберите задачу.");
                return;
            }

            // Создаем простое окно ввода текста
            var inputDialog = new InputDialog("Введите текст заметки (до 500 символов):");
            if (inputDialog.ShowDialog() == true)
            {
                var noteContent = inputDialog.ResponseText;

                if (string.IsNullOrWhiteSpace(noteContent))
                {
                    MessageBox.Show("Заметка не может быть пустой.");
                    return;
                }

                if (noteContent.Length > 500)
                {
                    MessageBox.Show("Заметка не должна превышать 500 символов.");
                    return;
                }

                var newNote = new Note
                {
                    Content = noteContent,
                    CreatedAt = DateTime.Now,
                    TaskId = selectedTask.Id,
                    UserId = 1 // Предположим, текущий пользователь
                };

                // Сохраняем в базу данных
                using (var context = new PlanerDbContext())
                {
                    context.Notes.Add(newNote);
                    context.SaveChanges();
                }

                // Обновляем список заметок
                Notes.Add(newNote);
                RefreshNotesDisplay();
            }
        }

        private void LoadNotes(int taskId)
        {
            using (var context = new PlanerDbContext())
            {
                Notes = new ObservableCollection<Note>(context.Notes.Where(n => n.TaskId == taskId).ToList());
            }

            RefreshNotesDisplay();
        }

        private void RefreshNotesDisplay()
        {
            // Очищаем StackPanel с заметками
            NotesStackPanel.Children.Clear();

            foreach (var note in Notes)
            {
                var noteBorder = CreateNoteBubble(note);
                NotesStackPanel.Children.Add(noteBorder);
            }
        }

        private Border CreateNoteBubble(Note note)
        {
            var border = new Border
            {
                Style = (Style)FindResource("ChatBubbleStyle"),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var stackPanel = new StackPanel();

            stackPanel.Children.Add(new TextBlock
            {
                Text = note.Content,
                TextWrapping = TextWrapping.Wrap
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = note.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                FontSize = 10,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Right
            });

            border.Child = stackPanel;

            return border;
        }

        private void TasksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedTask = TasksDataGrid.SelectedItem as TaskClass;

            if (selectedTask != null)
            {
                LoadNotes(selectedTask.Id);
            }
            else
            {
                NotesStackPanel.Children.Clear();
            }
        }


    }


}