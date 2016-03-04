using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Yorsh.Data;

namespace Yorsh.Model
{
    public class TaskList 
    {
        private readonly IList<TaskTable> _tasks;
        private readonly Dictionary<int, CategoryTable> _category;
        private readonly Dictionary<int, TaskTable> _taskDictionary;
        private readonly TaskEnumerator _enumerator;
        public TaskList(IEnumerable<TaskTable> tasks, IEnumerable<CategoryTable> categories, int currentPosition = 0)
        {
            _tasks = tasks.ToList();
            _taskDictionary = _tasks.ToDictionary(task => task.Id);
            _category = categories.ToDictionary(cat => cat.Id);
            _enumerator = new TaskEnumerator(this) { CurrentPosition = currentPosition };
        }

        public IEnumerable<TaskTable> Tasks
        {
            get { return _tasks; }
        }

        //public IEnumerator<TaskTable> GetEnumerator()
        //{
        //    return _enumerator;
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return _enumerator;
        //}

        public TaskEnumerator Enumerator
        {
            get { return _enumerator; }
        }
        public CategoryTable GetCategory(int categoryId)
        {
            return _category[categoryId];
        }

        public void Clear()
        {
            foreach (var taskTable in _tasks)
            {
                taskTable.Position = 0;
            }
            _enumerator.Reset();
        }

        public int Count
        {
            get { return _tasks.Count; }
        }

        public void Add(TaskTable task)
        {
            _tasks.Add(task);
        }

        public  TaskTable GetTask(int taskId)
        {
            return _taskDictionary[taskId];
        }

        public TaskTable this[int index]
        {
            get { return _tasks[index]; }
            set { _tasks[index] = value; }
        }

    }
}