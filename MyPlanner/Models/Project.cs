using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPlanner.Models
{
    public class Project
    {
        public int Id { get; set; }
        public bool IsCompleted { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public virtual List<TaskClass> Tasks { get; set; } = new List<TaskClass>();
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public DateTime Deadline { get; set; } 
        public Priority Priority { get; set; }
        public Category Category { get; set; } = Category.None;

        public bool IsOverdue => !IsCompleted && DateTime.UtcNow > Deadline;

        public void CompleteProject()
        {
            IsCompleted = true;
        }
    }
}
