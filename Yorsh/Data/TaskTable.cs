using System.Collections.Generic;
using Java.Lang;
using SQLite;
using Yorsh.Helpers;

namespace Yorsh.Data
{
    public class TaskTable : Object, ITable
    {
        public TaskTable()
        {

        }
        public TaskTable(int categoryId, string taskName, int score)
        {
            CategoryId = categoryId;
            TaskName = taskName;
            Score = score;
        }

        
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int CategoryId { get; set; }
        public string TaskName { get; set; }
        public int Score { get; set; }

        public int Position { get; set; }

        public bool IsBear
        {
            get { return CategoryId % 13 == 0; }
        }

        public bool IsEnabled { get; set; }
        public override string ToString()
        {
            return StringConst.TaskTable;
        }

        [Ignore]
        public string TableName { get { return ToString(); } }
    }
}
