using System;
using Yorsh.Data;

namespace Yorsh.Model.EventAgruments
{
    public class TaskPositionChangedEventArgs : EventArgs
    {
        public TaskPositionChangedEventArgs(TaskTable taskTable)
        {
            TaskTable = taskTable;
        }

        public TaskTable TaskTable { get; private set; }
    }
}