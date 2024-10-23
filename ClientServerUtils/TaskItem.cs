using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServerUtilsSharedProject
{
    public class TaskItem
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
    }
}
