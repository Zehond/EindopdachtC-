using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServerUtilsSharedProject
{
    public class TaskItem
    {
        public enum TaskState
        {
            ToDo,
            Progress,
            Done,
        }

        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required TaskState State { get; set; }
    }
}
