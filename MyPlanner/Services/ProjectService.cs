using Microsoft.EntityFrameworkCore;
using MyPlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPlanner.Services
{
    public class ProjectService : IDisposable
    {
        private readonly PlanerDbContext _dbContext;

        public ProjectService()
        {
            _dbContext = new PlanerDbContext();
        }

        // Метод для получения проектов пользователя
        public List<Project> GetProjectsByUser(int userId, bool isCompleted)
        {
            return _dbContext.Projects
                .Where(p => p.UserId == userId && p.IsCompleted == isCompleted)
                .Include(p => p.Tasks)
                .ToList();
        }

        // Метод для добавления проекта
        public void AddProject(Project project)
        {
            _dbContext.Projects.Add(project);
            _dbContext.SaveChanges();
        }

        // Метод для обновления проекта
        public void UpdateProject(Project project)
        {
            _dbContext.Entry(project).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }

        // Метод для удаления проекта
        public void DeleteProject(Project project)
        {
            _dbContext.Projects.Remove(project);
            _dbContext.SaveChanges();
        }

        public Project GetProjectByNameAndUserId(string name, int userId)
        {
            return _dbContext.Projects
                .FirstOrDefault(p => EF.Functions.ILike(p.Name, name) && p.UserId == userId);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }


    }
}
