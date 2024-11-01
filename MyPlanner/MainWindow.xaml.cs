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
            Loaded += MainWindow_Loaded; // Подписываемся на событие Loaded
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            rbActiveProjects.IsChecked = true; // Устанавливаем активные проекты по умолчанию
            LoadActiveProjects(); // Загружаем активные проекты
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
                        UserId = User.Id,
                        TaskId = selectedTask.Id
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
            NewProject newProjectWindow = new NewProject(User);
            if (newProjectWindow.ShowDialog() == true)
            {

                Project newProject = newProjectWindow.Project;

                // Создаем контейнер для проекта 
                Border projectBorder = CreateProjectBorder(newProject);

               

                // Добавляем проект в список
                User.Projects.Add(newProject);

                UpdateProjectDisplay(); // Обновляем отображение проектов
            }
        }

        private void UpdateProjectDisplay()
        {
            if (rbActiveProjects.IsChecked == true)
            {
                LoadActiveProjects();
            }
            else if (rbCompletedProjects.IsChecked == true)
            {
                LoadCompletedProjects();
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
            var selected_Project = selectedBorder.Tag as Project;

            // Проверка, чтобы сбрасывать задачу только при выборе другого проекта
            if (this.selectedProject != selected_Project)
            {
                // Сохраняем новый выбранный проект
                this.selectedProject = selected_Project;

                // Сбрасываем выбранную задачу
                selectedTask = null;

                // Сбрасываем цвет выделения всех строк в таблице задач
                ResetTaskSelection();

                // Очищаем заметки, так как задача сброшена
                NotesWrapPanel.Children.Clear();

                // Выделяем выбранный проект
                HighlightSelectedProject(selectedBorder);

                // Отображаем детали нового проекта
                DisplayProjectDetails(selected_Project);

                // Загружаем задачи для выбранного проекта
                LoadTasks(selected_Project);
            }

          
        }

        private void ResetTaskSelection()
        {
            // Сбрасываем цвет фона для всех строк в DataGrid
            foreach (var item in TasksDataGrid.Items)
            {
                if (TasksDataGrid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
                {
                    row.Background = Brushes.White; // Устанавливаем стандартный белый фон
                }
            }
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
            TaskWindow newTaskWindow= new TaskWindow(selectedProject);
            if (newTaskWindow.ShowDialog() == true && newTaskWindow.NewTask != null)
            {
             
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
            // Обновляем отображение
            TasksDataGrid.Items.Refresh();
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
                TaskWindow editTaskWindow = new TaskWindow(selectedProject,selectedTask); // Открываем окно редактирования задачи
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
                NewProject editProjectWindow = new NewProject(User,selectedProject); // Открываем окно редактирования проекта
                if (editProjectWindow.ShowDialog() == true)
                {
                    DisplayProjectDetails(selectedProject); // Обновляем детали проекта
                    foreach (Border projectBorder in ProjectsPanel.Children.OfType<Border>())
                    {
                        if (projectBorder.Tag == selectedProject)
                        {
                            // Обновляем содержимое выбранного проекта
                            UpdateProjectBorderContent(projectBorder, selectedProject);
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateProjectBorderContent(Border projectBorder, Project project)
        {
            if (projectBorder.Child is StackPanel projectContent)
            {
                // Обновляем текстовые блоки внутри StackPanel с новыми данными проекта
                ((TextBlock)projectContent.Children[0]).Text = project.Name;
                ((TextBlock)projectContent.Children[1]).Text = $"Приоритет: {project.Priority}";
                ((TextBlock)projectContent.Children[2]).Text = $"Категория: {project.Category}";
                ((TextBlock)projectContent.Children[3]).Text = $"Создан: {project.CreationDate:dd.MM.yyyy}";
                ((TextBlock)projectContent.Children[4]).Text = $"Дедлайн: {project.Deadline:dd.MM.yyyy}";
            }
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {

            if (selectedProject != null)
            {
                var result = MessageBox.Show("Вы действительно хотите удалить проект?", "Удаление проекта", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    // Удаляем проект из списка проектов пользователя
                    User.Projects.Remove(selectedProject);

                    // Очищаем панель проектов и перезагружаем её с оставшимися проектами
                    ProjectsPanel.Children.Clear();
                    foreach (var project in User.Projects)
                    {
                        ProjectsPanel.Children.Add(CreateProjectBorder(project));
                    }

                    // Очищаем таблицу задач и панель заметок
                    TasksDataGrid.ItemsSource = null;
                    NotesWrapPanel.Children.Clear();

                    // Сбрасываем выбранный проект и задачу
                    selectedProject = null;
                    selectedTask = null;

                    // Обновляем отображение (если нужно)
                    TasksDataGrid.Items.Refresh();
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

                var result = MessageBox.Show(noteTextBox.Name, "Редактировать заметку", MessageBoxButton.OKCancel);
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


        private void rbActiveChecked(object sender, RoutedEventArgs e)
        {
            LoadActiveProjects();
        }



        private void rbCompletedChecked(object sender, RoutedEventArgs e)
        {
            LoadCompletedProjects();
        }

        private void LoadActiveProjects()
        {
            if (ProjectsPanel == null)
            {
                MessageBox.Show("ProjectsPanel не инициализирован!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ProjectsPanel.Children.Clear();
            foreach (var project in User.Projects.Where(p => !p.IsCompleted))
            {
                ProjectsPanel.Children.Add(CreateProjectBorder(project));
            }
        }

        private void LoadCompletedProjects()
        {
            ProjectsPanel.Children.Clear();
            foreach (var project in User.Projects.Where(p => p.IsCompleted))
            {
                ProjectsPanel.Children.Add(CreateProjectBorder(project));
            }
        }

        private void CompleteProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProject == null)
            {
                MessageBox.Show("Выберите проект!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Проверка статуса завершения
            if (selectedProject.IsCompleted)
            {
                return;
            }

            // Завершаем проект
            selectedProject.CompleteProject();
            MessageBox.Show("Поздравляем, проект завершен!", "Завершение проекта", MessageBoxButton.OK, MessageBoxImage.Information);

            // Обновляем отображение проектов
            if (rbActiveProjects.IsChecked == true)
            {
                LoadActiveProjects();
            }
            else
            {
                LoadCompletedProjects();
            }
        }

    }
}