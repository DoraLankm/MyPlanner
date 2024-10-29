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
        Project selectedProject;
        TaskClass selectedTask;
        User User;

        public MainWindow()
        {
            InitializeComponent();
        }

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

            //автоматически создавать новый квадратик

            Border projectSquare = new Border
            {
                Width = 150,
                Height = 150,


                Child = new StackPanel
                {
                    Children =
                    {
                        new Label
                        {
                            Content =  "Название проекта"
                        },
                        
                    }
                }
            };
            ProjectsPanel.Children.Add(projectSquare);
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