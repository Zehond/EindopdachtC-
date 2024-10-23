using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServerUtilsSharedProject
{
    public enum StatusType {
        Get,
        Add,
        Remove,
        Edit,
    }

    public class NetworkJsonObject
    {
        public required StatusType Status { get; set; }
        public required TaskItem[] Items { get; set; }

    }
}
