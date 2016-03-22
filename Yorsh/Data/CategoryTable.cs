using SQLite;
using Yorsh.Helpers;

namespace Yorsh.Data
{
    public class CategoryTable : ITable
    {
        public CategoryTable()
        {

        }

        public CategoryTable(int id,string image)
        {
            Id = id;
            Image = image;
        }
        
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Image { get; set; }


        public bool IsEnabled { get; set; }

        [Ignore]
        public string TableName { get { return ToString(); } }
        public override string ToString()
        {
            return StringConst.CategoryTable;
        }
    }
}