using System;

namespace Yorsh.Data
{
    public static class Parser
    {
        public static CategoryTable ParseCategory(string parseString)
        {
            var values = parseString.Split(';');
            return new CategoryTable(int.Parse(values[0]), values[1], values[2]);
        }

        public static TaskTable ParseTask(string parseString)
        {
            var values = parseString.Split(';');
            return new TaskTable(int.Parse(values[0]), values[1], int.Parse(values[2]));
        }

        public static BonusTable ParseBonus(string parseString)
        {
            var values = parseString.Split(';');
            return new BonusTable(values[0], int.Parse(values[1]));
        }
    }
}