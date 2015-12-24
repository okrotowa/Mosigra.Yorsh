using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Android.Content;
using SQLite;
using Exception = System.Exception;
using Path = System.IO.Path;

namespace Yorsh.Model
{
    public class Rep
    {
        private static Rep _instance;
        private PlayerList _players;
        private TaskList _tasks;
        private IList<BonusTable> _bonuses;

        private Rep()
        {
            _players = new PlayerList();
        }


        public static Rep Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new Rep();
                return _instance;
            }
        }

        public async Task InitializeRepositoryAsync()
        {
            if (System.IO.File.Exists(GetPlayersFile()))
            {
                try
                {
                    using (var playersFileStream = System.IO.File.Open(GetPlayersFile(), FileMode.Open))
                    {
                        var bin = new BinaryFormatter();
                        _players = new PlayerList((IList<Player>)bin.Deserialize(playersFileStream));
                    }
                }
                catch (Exception)
                {
                    _players = new PlayerList();
                }
            }
            else _players = new PlayerList();
            await TaskGenerateAsync();
            await BonusGenerateAsync();
        }

        private string GetPlayersFile()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return Path.Combine(documentsPath, "players.bin");
        }

        public void SavePlayers()
        {
            using (var playersFileStream = System.IO.File.Open(GetPlayersFile(), FileMode.OpenOrCreate))
            {
                var bin = new BinaryFormatter();
                var list = _players.Items;
                bin.Serialize(playersFileStream, list);
            }
        }

        public string DataBaseFile
        {
            get
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                return Path.Combine(documentsPath, "Yoursh.db");
            }
        }

        public int AllBonusCount
        {
            get { return 81; }
        }

        public int AllTaskCount
        {
            get { return 174; }
        }
        public PlayerList Players
        {
            get
            {
                return _players;
            }
        }
        public TaskList Tasks { get { return _tasks; } }
        public IList<BonusTable> Bonuses { get { return _bonuses; } }

        public async Task TaskGenerateAsync()
        {
            var connect = new SQLiteAsyncConnection(DataBaseFile);
            var taskList = await connect.Table<TaskTable>().ToListAsync();
            if (_tasks != null)
                foreach (var taskTable in taskList.Where(taskTable => !taskList.Contains(taskTable)))
                {
                    _tasks.Add(taskTable);
                }
            else
            {
                var categoryList = await connect.Table<CategoryTable>().ToListAsync();
                _tasks = new TaskList(taskList, categoryList);
            }
        }

        public async Task BonusGenerateAsync()
        {
            var connect = new SQLiteAsyncConnection(DataBaseFile);
            _bonuses = await connect.Table<BonusTable>().ToListAsync();
        }

        public void Clear()
        {
            _players.Reset();
            _tasks.Clear();
        }
    }
}
