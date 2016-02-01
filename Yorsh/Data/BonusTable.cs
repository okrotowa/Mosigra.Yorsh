using SQLite;
using Yorsh.Helpers;

namespace Yorsh.Data
{
    public class BonusTable : ITable
    {
        public BonusTable()
        {
            
        }
        public BonusTable(string bonusName, int score)
        {
            BonusName = bonusName;
            Score = score;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string BonusName { get; set; }
        private int Score { get; set; }
        public string TableName { get { return ToString(); }}
        public bool IsEnabled { get; set; }
        public override string ToString()
        {
            return StringConst.BonusTable;
        }
    }
}