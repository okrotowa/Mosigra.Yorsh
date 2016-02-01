using SQLite;

namespace Yorsh.Helpers
{
    public static class SqLiteExtensions
    {
        public static bool TableExists<T>(this SQLiteConnection connection)
        {
           const string cmdText = "SELECT name FROM sqlite_master WHERE type='table' AND name=?";
            var cmd = connection.CreateCommand(cmdText, typeof(T).Name);
            return cmd.ExecuteScalar<string>() != null;
        }
    }
}