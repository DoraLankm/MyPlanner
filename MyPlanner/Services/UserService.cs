using MyPlanner.Helper;
using MyPlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPlanner.Services
{
    public class UserService : IDisposable
    {
        private readonly PlanerDbContext _dbContext;

        public UserService()
        {
            _dbContext = new PlanerDbContext();
        }

        // Метод для проверки наличия пользователя по логину
        public bool IsUserExists(string login)
        {
            return _dbContext.Users.Any(u => u.Login == login);
        }

        // Метод для добавления нового пользователя
        public void AddUser(User user)
        {
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }

        // Метод для получения пользователя по логину
        public User GetUserByLogin(string login)
        {
            return _dbContext.Users.FirstOrDefault(u => u.Login == login);
        }

        // Метод для проверки пароля пользователя
        public bool VerifyUserPassword(string login, string password)
        {
            var user = GetUserByLogin(login);

            if (user == null)
                return false;

            return PasswordHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt);
        }

        // Освобождение ресурсов
        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
