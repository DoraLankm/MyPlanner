using MyPlanner.Helper;
using MyPlanner.Models;
using MyPlanner.Services;
using System;
using System.Windows;

namespace MyPlanner
{
    public partial class Login : Window
    {
        private UserService _userService;

        public Login()
        {
            InitializeComponent();
            _userService = new UserService();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Проверяем, существует ли пользователь
                if (!_userService.IsUserExists(login))
                {
                    MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверяем пароль
                bool isPasswordValid = _userService.VerifyUserPassword(login, password);

                if (!isPasswordValid)
                {
                    MessageBox.Show("Неверный пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Получаем пользователя
                User user = _userService.GetUserByLogin(login);

                // Авторизация успешна, открываем главное окно
                MainWindow mainWindow = new MainWindow(user);
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _userService.Dispose();
        }
    }
}
