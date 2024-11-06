using Microsoft.EntityFrameworkCore;
using MyPlanner.Models;
using MyPlanner.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TaskStatus = MyPlanner.Models.TaskStatus;

namespace MyPlanner
{
    public partial class MainWindow : Window
    {
        private User _user;
        private Project _selectedProject;
        private TaskClass _selectedTask;
        private ProjectService _projectService;
        private TaskService _taskService;
        private NoteService _noteService;
        private List<Project> _projects = new List<Project>();

        public MainWindow(User user)
        {
            _user = user;
            InitializeComponent();

            _projectService = new ProjectService();
            _taskService = new TaskService();
            _noteService = new NoteService();

            Loaded += MainWindow_Loaded;
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            rbActiveProjects.IsChecked = true;
        }

        private void LoadProjects(bool isCompleted)
        {
            try
            {
                ProjectsPanel.Children.Clear();

                _projects = _projectService.GetProjectsByUser(_user.Id, isCompleted);

                foreach (var project in _projects)
                {
                    ProjectsPanel.Children.Add(CreateProjectBorder(project));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке проектов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTask == null)
            {
                MessageBox.Show("Задача не выбрана", "Ошибка");
                return;
            }

            if (_selectedProject.IsCompleted)
            {
                MessageBox.Show("Вы завершили проект", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime currentDate = DateTime.UtcNow;

            TextBox noteTextBox = new TextBox
            {
                Style = (Style)FindResource("NoteTextBoxStyle"),
                MaxLength = 250
            };

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
                    try
                    {
                        Note newNote = new Note
                        {
                            Content = noteTextBox.Text,
                            CreatedAt = currentDate,
                            UserId = _user.Id,
                            TaskId = _selectedTask.Id
                        };
                        //_selectedTask.Notes.Add(newNote);

                        _noteService.AddNote(newNote);

                        // Обновляем заметку после сохранения
                        NotesWrapPanel.Children.Remove(tempNoteBubble);
                        NotesWrapPanel.Children.Add(CreateNoteBubble(newNote));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при добавлении заметки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            };
        }

        private void LoadNotes(TaskClass task)
        {
            try
            {
                NotesWrapPanel.Children.Clear();

                if (task == null) return;

                var notes = _noteService.GetNotesByTask(task.Id);

                foreach (var note in notes)
                {
                    Border noteBubble = CreateNoteBubble(note);
                    NotesWrapPanel.Children.Add(noteBubble);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заметок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NewProject newProjectWindow = new NewProject(_user,_projectService);
                if (newProjectWindow.ShowDialog() == true)
                {
                    Project newProject = newProjectWindow.Project;
                    newProject.UserId = _user.Id;

                    _projectService.AddProject(newProject);

                    UpdateProjectDisplay();

                    SelectProject(newProject);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateProjectDisplay()
        {
            if (rbActiveProjects.IsChecked == true)
            {
                LoadProjects(false);
            }
            else if (rbCompletedProjects.IsChecked == true)
            {
                LoadProjects(true);
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
                Tag = project
            };

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

            projectContent.Children.Add(projectName);
            projectContent.Children.Add(projectPriority);
            projectContent.Children.Add(projectCategory);
            projectContent.Children.Add(projectCreationDate);
            projectContent.Children.Add(projectDeadline);

            projectBorder.Child = projectContent;

            projectBorder.MouseLeftButtonUp += ProjectBorder_MouseLeftButtonUp;

            return projectBorder;
        }

        private void ProjectBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var selectedBorder = sender as Border;
            var selected_Project = selectedBorder.Tag as Project;

            if (this._selectedProject != selected_Project)
            {
                this._selectedProject = selected_Project;

                _selectedTask = null;

                ResetTaskSelection();

                NotesWrapPanel.Children.Clear();

                HighlightSelectedProject(selectedBorder);

                DisplayProjectDetails(selected_Project);

                LoadTasks();
            }
        }

        private void ResetTaskSelection()
        {
            foreach (var item in TasksDataGrid.Items)
            {
                if (TasksDataGrid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
                {
                    row.Background = Brushes.White;
                }
            }
        }

        private void HighlightSelectedProject(Border selectedBorder)
        {
            foreach (var child in ProjectsPanel.Children)
            {
                if (child is Border border)
                {
                    border.Background = (Brush)this.Resources["Color4"];
                }
            }

            selectedBorder.Background = (Brush)this.Resources["Color3"];
        }

        private void DisplayProjectDetails(Project project)
        {
            txtProjectTitle.Text = project.Name;
            txtProjectDescription.Text = project.Description;
        }

        private void SelectProject(Project project)
        {
            _selectedProject = project;
            _selectedTask = null;

            ResetTaskSelection();

            NotesWrapPanel.Children.Clear();

            // Найти Border, соответствующий проекту
            Border selectedBorder = null;
            foreach (var child in ProjectsPanel.Children)
            {
                if (child is Border border && border.Tag is Project proj)
                {
                    if (proj.Id == project.Id)
                    {
                        selectedBorder = border;
                        break;
                    }
                }
            }

            if (selectedBorder != null)
            {
                HighlightSelectedProject(selectedBorder);
            }

            DisplayProjectDetails(project);

            LoadTasks();
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject == null)
            {
                MessageBox.Show("Проект не выбран", "Ошибка");
                return;
            }
            if (_selectedProject.IsCompleted)
            {
                MessageBox.Show("Вы завершили проект", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                TaskWindow newTaskWindow = new TaskWindow(_selectedProject);
                if (newTaskWindow.ShowDialog() == true && newTaskWindow.NewTask != null)
                {
                    TaskClass newTask = newTaskWindow.NewTask;
                    newTask.ProjectId = _selectedProject.Id;

                    _taskService.AddTask(newTask);

                    LoadTasks();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTasks()
        {
            try
            {
                if (_selectedProject == null)
                {
                    TasksDataGrid.ItemsSource = null;
                    NotesWrapPanel.Children.Clear();
                    _selectedTask = null;
                    return;
                }

                bool isCompleted = rbCompletedTasks.IsChecked == true;
                var tasks = _taskService.GetTasksByProject(_selectedProject.Id, isCompleted);

                TasksDataGrid.ItemsSource = tasks;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке задач: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TasksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTask = (TaskClass)TasksDataGrid.SelectedItem;

            if (_selectedTask != null)
            {
                HighlightSelectedTask(_selectedTask);
                LoadNotes(_selectedTask);
            }
            else
            {
                ResetTaskSelection();
                NotesWrapPanel.Children.Clear();
            }
        }

        private Border CreateNoteBubble(Note note)
        {
            TextBlock noteTextBlock = new TextBlock
            {
                Text = note.Content,
                TextWrapping = TextWrapping.Wrap
            };

            TextBlock dateTextBlock = new TextBlock
            {
                Text = note.CreatedAt.ToString(),
                Style = (Style)FindResource("DateTextBlockStyle")
            };

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
            foreach (var item in TasksDataGrid.Items)
            {
                if (TasksDataGrid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
                {
                    row.Background = item == task ? Brushes.LightBlue : Brushes.White;
                }
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTask != null)
            {
                try
                {
                    TaskWindow editTaskWindow = new TaskWindow(_selectedProject, _selectedTask);
                    if (editTaskWindow.ShowDialog() == true)
                    {
                        _taskService.UpdateTask(_selectedTask);

                        // Проверяем, завершены ли все задачи в проекте
                        if (_taskService.GetTasksByProject(_selectedProject.Id, false).Count == 0)
                        {
                            _selectedProject.IsCompleted = true;
                            _projectService.UpdateProject(_selectedProject);

                            MessageBox.Show("Поздравляем, проект завершен!", "Завершение проекта", MessageBoxButton.OK, MessageBoxImage.Information);
                            _selectedProject = null;
                            UpdateProjectDisplay();
                            LoadTasks();
                        }
                        else
                        {
                            LoadTasks();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при редактировании задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTask != null)
            {
                var result = MessageBox.Show("Вы действительно хотите удалить задачу?", "Подтверждение удаления", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _taskService.DeleteTask(_selectedTask);

                        _selectedTask = null;
                        LoadTasks();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void EditProject_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject != null)
            {
                try
                {
                    NewProject editProjectWindow = new NewProject(_user, _selectedProject, _projectService);
                    if (editProjectWindow.ShowDialog() == true)
                    {
                        _projectService.UpdateProject(_selectedProject);

                        DisplayProjectDetails(_selectedProject);
                        UpdateProjectDisplay();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при редактировании проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject != null)
            {
                var result = MessageBox.Show("Вы действительно хотите удалить проект?", "Удаление проекта", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _projectService.DeleteProject(_selectedProject);

                        _selectedProject = null;
                        UpdateProjectDisplay();

                        TasksDataGrid.ItemsSource = null;
                        NotesWrapPanel.Children.Clear();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTask != null)
            {
                try
                {
                    var notes = _noteService.GetNotesByTask(_selectedTask.Id);

                    if (notes.Count > 0)
                    {
                        var noteToDelete = notes[^1];

                        var result = MessageBox.Show("Вы действительно хотите удалить заметку?", "Подтверждение удаления", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            _noteService.DeleteNote(noteToDelete);

                            LoadNotes(_selectedTask);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Нет заметок для удаления", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении заметки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void rbActiveChecked(object sender, RoutedEventArgs e)
        {
            LoadProjects(false);
        }

        private void rbCompletedChecked(object sender, RoutedEventArgs e)
        {
            LoadProjects(true);
        }

        private void CompleteProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject == null)
            {
                MessageBox.Show("Выберите проект!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                _selectedProject.IsCompleted = true;

                // Обновляем проект в базе данных через сервис
                _projectService.UpdateProject(_selectedProject);

                MessageBox.Show("Проект успешно завершен!", "Завершение проекта", MessageBoxButton.OK, MessageBoxImage.Information);

                _selectedProject = null;
                _selectedTask = null;

                UpdateProjectDisplay();
                LoadTasks();

                txtProjectTitle.Text = string.Empty;
                txtProjectDescription.Text = string.Empty;
                NotesWrapPanel.Children.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при завершении проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void rbActiveTasksChecked(object sender, RoutedEventArgs e)
        {
            if (_selectedProject != null)
            {
                LoadTasks();
            }
        }

        private void ExitProgram_Click(object sender, RoutedEventArgs e)
        {
            StartWindow startWindow = new StartWindow();
            startWindow.Show();
            this.Close();
        }

        private void rbCompletedTasksChecked(object sender, RoutedEventArgs e)
        {
            if (_selectedProject != null)
            {
                LoadTasks();
            }
        }

        private void SearchProjects(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return;
            }

            // Поиск проекта по названию
            var matchingProject = _projects.FirstOrDefault(p => p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            if (matchingProject != null)
            {
                SelectProject(matchingProject);
            }
            else
            {
                MessageBox.Show("Совпадений не найдено", "Ошибка");
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();
            SearchProjects(searchText);
        }
    }
}
