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

        private void TasksDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            isEditingTask = true;
        }

        private void TasksDataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (TasksDataGrid.CurrentItem is TaskClass editedTask)
            {
                // Получаем текущую ячейку и проверяем, какой столбец редактируется
                var column = TasksDataGrid.CurrentCell.Column;
                if (column != null)
                {
                    string header = column.Header.ToString();

                    if (header == "Название задачи")
                    {
                        // Проверяем уникальность названия задачи
                        if (!IsTaskNameUnique(editedTask.Title))
                        {
                            MessageBox.Show("Название задачи должно быть уникальным.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            editedTask.Title = string.Empty; // Очищаем поле, если оно не уникально
                            return;
                        }
                    }
                    else if (header == "Описание задачи")
                    {
                        // Ограничиваем длину описания задачи
                        if (editedTask.Description != null && editedTask.Description.Length > 300)
                        {
                            MessageBox.Show("Описание задачи не должно превышать 300 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            editedTask.Description = editedTask.Description.Substring(0, 300); // Обрезаем текст до 300 символов
                        }
                    }

                    // Сохраняем изменения
                    SaveChangesToTasks();
                }
            }
        }

        private void TasksDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            // Проверяем, действительно ли была редактируемая операция
            if (isEditingTask && e.Row.Item is TaskClass editedTask)
            {
                isEditingTask = false; // Сбрасываем флаг, так как редактирование завершено

                // Проверяем уникальность названия задачи, если изменяется Title
                if (!IsTaskNameUnique(editedTask.Title))
                {
                    MessageBox.Show("Название задачи должно быть уникальным.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    editedTask.Title = string.Empty; // Сбрасываем значение, если не уникально
                    return;
                }

                // Проверяем длину описания, если оно изменяется
                if (editedTask.Description != null && editedTask.Description.Length > 300)
                {
                    MessageBox.Show("Описание задачи не должно превышать 300 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    editedTask.Description = editedTask.Description.Substring(0, 300); // Обрезаем до 300 символов
                }

                // Даем завершить редактирование строки перед обновлением
                TasksDataGrid.Dispatcher.InvokeAsync(() =>
                {
                    SaveChangesToTasks();
                    TasksDataGrid.Items.Refresh(); // Обновляем интерфейс
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }



        private void SaveChangesToTasks()
        {
            
            TasksDataGrid.Items.Refresh(); // Обновляем отображение в DataGrid
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


    }
}