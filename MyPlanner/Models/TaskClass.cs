using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPlanner.Models
{
    public class TaskClass
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string? Description { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.NotStarted;

        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime Deadline { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public List<Note> Notes { get; set; } = new List<Note>();
    }
}
