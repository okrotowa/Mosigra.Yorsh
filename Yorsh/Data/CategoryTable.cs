using SQLite;
using Yorsh.Helpers;

namespace Yorsh.Data
{
    public class CategoryTable : ITable
    {
        public CategoryTable()
        {

        }

        public CategoryTable(int id, string categoryName,string imageName)
        {
            Id = id;
            ImageName = imageName;
            CategoryName = categoryName;
        }
        
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string ImageName { get; set; }

        public string CategoryName { get; set; }

        public bool IsEnabled { get; set; }

        [Ignore]
        public string TableName { get { return ToString(); } }
        public override string ToString()
        {
            return StringConst.CategoryTable;
        }
    }
}