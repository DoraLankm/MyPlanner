using MyPlanner.Helper;
using MyPlanner.Models;
using MyPlanner.Services;
using System;
using System.Windows;

namespace MyPlanner
{
    public partial class Registration : Window
    {
        private UserService _userService;

        public Registration()
        {
            InitializeComponent();
            _userService = new UserService();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Все поля обязательны для заполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Проверяем, существует ли пользователь с таким логином
                if (_userService.IsUserExists(login))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Хэшируем пароль
                PasswordHelper.CreatePasswordHash(password, out string passwordHash, out string passwordSalt);

                // Создаем нового пользователя
                User newUser = new User
                {
                    Login = login,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Name = name
                };

                // Добавляем пользователя через сервис
                _userService.AddUser(newUser);

                MessageBox.Show("Регистрация успешно завершена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Переходим на окно входа
                Login loginWindow = new Login();
                loginWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message} { ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _userService.Dispose();
        }
    }
}
