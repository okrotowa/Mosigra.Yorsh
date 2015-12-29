using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Android.Content;
using SQLite;
using Exception = System.Exception;
using Path = System.IO.Path;

namespace Yorsh.Model
{
    public sealed class Rep
    {
        private static Rep _instance;
        private PlayerList _players;
        private TaskList _tasks;
        private IList<BonusTable> _bonuses;

        private Rep()
        {
            _players = new PlayerList();
        }

        public event EventHandler DatabaseChanged;

        private void OnDatabaseChanged()
        {
            var handler = DatabaseChanged;
            if (handler != null) handler(this, EventArgs.Empty);
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
            await PlayersGenerateAsync();
            await TaskGenerateAsync();
            await BonusGenerateAsync();
        }

        private async Task PlayersGenerateAsync()
        {
            if (File.Exists(GetPlayersFile()))
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
        }

        private string GetPlayersFile()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return Path.Combine(documentsPath, "players.bin");
        }

        public void SavePlayers()
        {
            using (var playersFileStream = File.Open(GetPlayersFile(), FileMode.OpenOrCreate))
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
            get { return 80; }
        }

        public int AllTaskCount
        {
            get { return 170; }
        }
        public PlayerList Players
        {
            get
            {
                return _players;
            }
        }

        public TaskList Tasks
        {
            get { return _tasks; }
            private set
            {
                _tasks = value;
                OnDatabaseChanged();
            }
        }
        public IList<BonusTable> Bonuses
        {
            get
            {
                return _bonuses;
            }
            private set
            {
                _bonuses = value;
                OnDatabaseChanged();
            }
        }

        public async Task TaskGenerateAsync()
        {
            var connect = new SQLiteAsyncConnection(DataBaseFile);
            var enumerator = Tasks == null ? 0 :  Tasks.Enumerator.CurrentPosition;
            var categoryList = await connect.Table<CategoryTable>().ToListAsync();
            var taskList = await connect.Table<TaskTable>().ToListAsync();
            Tasks = new TaskList(taskList, categoryList);
            Tasks.Enumerator.SetCurrent(enumerator);
        }

        public async Task BonusGenerateAsync()
        {
            var connect = new SQLiteAsyncConnection(DataBaseFile);
            Bonuses = await connect.Table<BonusTable>().ToListAsync();
        }

        public void Clear(ISharedPreferencesEditor editor)
        {
            _players.Reset();
            Tasks.Clear();
            editor.PutInt("currentPlayer", 0);
            editor.PutInt("currentTask", 0);
            editor.Commit();
        }

        public void SaveContext(ISharedPreferencesEditor editor)
        {
            editor.PutInt("currentPlayer", Instance.Players.CurrentPosition);
            editor.PutInt("currentTask", Tasks.Enumerator.CurrentPosition);
            editor.Commit();
        }
    }
}
