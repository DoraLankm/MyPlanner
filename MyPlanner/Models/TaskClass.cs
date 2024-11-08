﻿using System;
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

        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public DateTime Deadline { get; set; }

        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
        public virtual List<Note> Notes { get; set; } = new List<Note>();

        public bool IsOverdue => Status != TaskStatus.Completed && DateTime.Now > Deadline;
    }
}
