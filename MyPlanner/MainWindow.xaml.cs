using MyPlanner.Models;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
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
        private bool isEditingTask = false;

        User User; //авторизованный пользователь 
        private Project selectedProject; //выбранный проект
        private TaskClass selectedTask; //выбранная задача (у активной задачи границы становятся толще)

        public MainWindow()
        {
            User = new User() //имитация пользователя
            {
                Id = 1,
                Name = "Me"
            };
            InitializeComponent();
        }

        bool IsProjectNameUnique(string name)
        {

            foreach (var project in User.Projects)
            {
                if (project.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        bool IsTaskNameUnique(string name)
        {

            foreach (var task in selectedProject.Tasks)
            {
                if (task.Title.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private void AddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTask == null)
            {
                MessageBox.Show("Задача не выбрана", "Ошибка");
                return;
            }

            DateTime currentDate = DateTime.Now;

            // Создаем TextBox для ввода новой заметки
            TextBox noteTextBox = new TextBox
            {
                Style = (Style)FindResource("NoteTextBoxStyle")
            };

            noteTextBox.TextChanged += (s, ev) =>
            {
                if (noteTextBox.Text.Length > 250)
                {
                    noteTextBox.Text = noteTextBox.Text.Substring(0, 250);
                    noteTextBox.CaretIndex = noteTextBox.Text.Length;
                }
            };

            // Пузырек для новой заметки, пока она вводится
            Border tempNoteBubble = new Border
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
                    Text = currentDate.ToString()
                }
            }
                }
            };

            NotesWrapPanel.Children.Add(tempNoteBubble);
            noteTextBox.Focus();

            noteTextBox.LostFocus += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(noteTextBox.Text))
                {
                    NotesWrapPanel.Children.Remove(tempNoteBubble);
                }
                else
                {
                    Note newNote = new Note
                    {
                        Content = noteTextBox.Text,
                        CreatedAt = currentDate,
                    };
                    selectedTask.Notes.Add(newNote);

                    // Обновляем заметку после сохранения
                    NotesWrapPanel.Children.Remove(tempNoteBubble);
                    NotesWrapPanel.Children.Add(CreateNoteBubble(newNote));
                }
            };
        }


        private void LoadNotes(TaskClass task)
        {
            // Очищаем панель заметок перед загрузкой новых
            NotesWrapPanel.Children.Clear();

            if (task == null || task.Notes == null) return;

            foreach (var note in task.Notes)
            {
                // Используем метод для создания пузырька заметки
                Border noteBubble = CreateNoteBubble(note);
                NotesWrapPanel.Children.Add(noteBubble);
            }
        }

        private void AddProjectButton_Click(object sender, RoutedEventArgs e) //добавить проект 
        {
            NewProject newProjectWindow = new NewProject();
            if (newProjectWindow.ShowDialog() == true)
            {
                if (!IsProjectNameUnique(newProjectWindow.Project.Name))
                {
                    MessageBox.Show("Проект с таким названием уже существует!");
                    return;
                }
                Project newProject = newProjectWindow.Project;

                // Создаем контейнер для проекта 
                Border projectBorder = CreateProjectBorder(newProject);

                // Добавляем проект в WrapPanel
                ProjectsPanel.Children.Add(projectBorder);

                // Добавляем проект в список
                User.Projects.Add(newProject);
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
            LoadTasks(selectedProject);

            selectedTask = null;
            NotesWrapPanel.Children.Clear(); //очистка заметок
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

       

        private void AddTaskButton_Click(object sender, RoutedEventArgs e) //добавить задачу 
        {
            if (selectedProject == null)
            {
                MessageBox.Show("Проект не выбран", "Ошибка");
                return;
            }
            TaskWindow newTaskWindow= new TaskWindow();
            if (newTaskWindow.ShowDialog() == true && newTaskWindow.NewTask != null)
            {
                if (!IsTaskNameUnique(newTaskWindow.NewTask.Title))
                {
                    MessageBox.Show("Задача уже существует!");
                    return;
                }
                // Добавляем задачу в список задач проекта
                selectedProject.Tasks.Add(newTaskWindow.NewTask);

                // Обновляем DataGrid для отображения новой задачи
                LoadTasks(selectedProject);
                TasksDataGrid.Items.Refresh(); // Обновляем DataGrid
            }
        }

        private void LoadTasks(Project project)
        {
            // Устанавливаем задачи выбранного проекта как источник данных для DataGrid
            TasksDataGrid.ItemsSource = project.Tasks;
        }


        private void TasksDataGrid_SelectionChanged(object sender, RoutedEventArgs e) //изменение выбранной задачи
        {
            selectedTask = (TaskClass)TasksDataGrid.SelectedItem;
            HighlightSelectedTask(selectedTask);
            LoadNotes(selectedTask);
        }

       
       

    
        private Border CreateNoteBubble(Note note)
        {
            // Текст заметки, здесь используем TextBlock, без стиля NoteTextBoxStyle
            TextBlock noteTextBlock = new TextBlock
            {
                Text = note.Content,
                TextWrapping = TextWrapping.Wrap // добавим перенос текста, если нужно
            };

            // Дата создания заметки
            TextBlock dateTextBlock = new TextBlock
            {
                Text = note.CreatedAt.ToString(),
                Style = (Style)FindResource("DateTextBlockStyle")
            };

            // Создаем пузырек заметки
            Border noteBubble = new Border
            {
                Style = (Style)FindResource("ChatBubbleStyle"),
                Child = new StackPanel
                {
                    Children = { noteTextBlock, dateTextBlock }
                }
            };

            return noteBubble;
        }


        private void HighlightSelectedTask(TaskClass task)
        {
            if (task != null)
            {
                foreach (var item in TasksDataGrid.Items)
                {
                    var row = TasksDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                    if (row != null)
                    {
                        row.Background = item == task ? Brushes.LightBlue : Brushes.White;
                    }
                }
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTask != null)
            {
                TaskWindow editTaskWindow = new TaskWindow(selectedTask); // Открываем окно редактирования задачи
                if (editTaskWindow.ShowDialog() == true)
                {
                    LoadTasks(selectedProject); // Обновляем список задач после редактирования
                }
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTask != null && selectedProject != null)
            {
                var result = MessageBox.Show("Вы действительно хотите удалить задачу?", "Подтверждение удаления", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    selectedProject.Tasks.Remove(selectedTask); // Удаляем задачу из списка
                    selectedTask = null;
                    LoadTasks(selectedProject); // Обновляем таблицу задач
                }
            }
        }

        private void EditProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProject != null)
            {
                NewProject editProjectWindow = new NewProject(selectedProject); // Открываем окно редактирования проекта
                if (editProjectWindow.ShowDialog() == true)
                {
                    DisplayProjectDetails(selectedProject); // Обновляем детали проекта
                }
            }
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProject != null)
            {
                var result = MessageBox.Show("Вы действительно хотите удалить проект?", "Подтверждение удаления", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    User.Projects.Remove(selectedProject); // Удаляем проект из списка
                    ProjectsPanel.Children.Clear(); // Обновляем панель проектов
                    foreach (var project in User.Projects)
                    {
                        ProjectsPanel.Children.Add(CreateProjectBorder(project));
                    }
                    selectedProject = null;
                }
            }
        }

        private void EditNote_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTask != null && selectedTask.Notes != null && selectedTask.Notes.Count > 0)
            {
                var note = selectedTask.Notes.Last(); // Пример: редактируем последнюю заметку
                TextBox noteTextBox = new TextBox
                {
                    Text = note.Content,
                    AcceptsReturn = true,
                    MaxLength = 250,
                    Width = 300
                };

                var result = MessageBox.Show(noteTextBox, "Редактировать заметку", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    note.Content = noteTextBox.Text;
                    LoadNotes(selectedTask); // Обновляем список заметок
                }
            }
        }

        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTask != null && selectedTask.Notes != null && selectedTask.Notes.Count > 0)
            {
                var note = selectedTask.Notes.Last(); // Пример: удаляем последнюю заметку
                var result = MessageBox.Show("Вы действительно хотите удалить заметку?", "Подтверждение удаления", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    selectedTask.Notes.Remove(note);
                    LoadNotes(selectedTask); // Обновляем список заметок
                }
            }
        }



    }
}