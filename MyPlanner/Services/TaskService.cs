using Microsoft.EntityFrameworkCore;
using MyPlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskStatus = MyPlanner.Models.TaskStatus;

namespace MyPlanner.Services
{
    public class TaskService : IDisposable
    {
        private readonly PlanerDbContext _dbContext;

        public TaskService()
        {
            _dbContext = new PlanerDbContext();
        }

        // Метод для получения задач проекта
        public List<TaskClass> GetTasksByProject(int projectId, bool isCompleted)
        {
            var query = _dbContext.Tasks.Where(t => t.ProjectId == projectId);

            if (isCompleted)
                query = query.Where(t => t.Status == TaskStatus.Completed);
            else
                query = query.Where(t => t.Status != TaskStatus.Completed);

            return query.ToList();
        }

        // Метод для добавления задачи
        public void AddTask(TaskClass task)
        {
            _dbContext.Tasks.Add(task);
            _dbContext.SaveChanges();
        }

        // Метод для обновления задачи
        public void UpdateTask(TaskClass task)
        {
            _dbContext.Entry(task).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }

        // Метод для удаления задачи
        public void DeleteTask(TaskClass task)
        {
            _dbContext.Tasks.Remove(task);
            _dbContext.SaveChanges();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
