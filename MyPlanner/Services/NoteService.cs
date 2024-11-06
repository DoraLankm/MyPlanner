using MyPlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPlanner.Services
{
    public class NoteService : IDisposable
    {
        private readonly PlanerDbContext _dbContext;

        public NoteService()
        {
            _dbContext = new PlanerDbContext();
        }

        // Метод для получения заметок задачи
        public List<Note> GetNotesByTask(int taskId)
        {
            return _dbContext.Notes
                .Where(n => n.TaskId == taskId)
                .OrderBy(n => n.CreatedAt)
                .ToList();
        }

        // Метод для добавления заметки
        public void AddNote(Note note)
        {
            _dbContext.Notes.Add(note);
            _dbContext.SaveChanges();
        }

        // Метод для удаления заметки
        public void DeleteNote(Note note)
        {
            _dbContext.Notes.Remove(note);
            _dbContext.SaveChanges();
        }


        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
