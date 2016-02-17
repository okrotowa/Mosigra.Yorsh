using System;
using System.Collections;
using System.Collections.Generic;
using Yorsh.Data;
using Yorsh.Model.EventAgruments;

namespace Yorsh.Model
{
    public class TaskEnumerator : IEnumerator<TaskTable>
    {
        private readonly TaskList _taskList;
        private int _current;

        public event EventHandler<TaskPositionChangedEventArgs> TaskPositionChanged;
        public void OnTaskPositionChanged(TaskPositionChangedEventArgs e)
        {
            var handler = TaskPositionChanged;
            if (handler != null) handler(this, e);
        }

        public TaskEnumerator(TaskList taskList)
        {
            _taskList = taskList;
            _current = 0;
        }

        public bool MoveNext()
        {
            if (CurrentPosition >= _taskList.Count - 1) return false;
            CurrentPosition = CurrentPosition + 1;
            return true;
        }

        public void Reset()
        {
            _current = 0;
        }

        public int CurrentPosition
        {
            get { return _current; }
            set
            {
                _current = value;
                Current.Position = _current;
                OnTaskPositionChanged(new TaskPositionChangedEventArgs(Current));
            }
        }

        public TaskTable Current
        {
            get { return _taskList[_current]; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public void Dispose()
        {
        }
    }
}