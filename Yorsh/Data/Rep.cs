using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Android.App;
using Android.Gms.Analytics;
using Yorsh.Helpers;
using Yorsh.Model;

namespace Yorsh.Data
{
    public sealed class Rep
    {
        private static Rep _instance;
        private PlayerList _players;
        public Tracker GaTracker { get; set; }
        public GoogleAnalytics GaInstance { get; set; }

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

        private DatabaseHelper _helper;
        public static DatabaseHelper DatabaseHelper
        {
            get { return Instance._helper; }
        }
        public void InitDataBase(Application application)
        {
            _helper = new DatabaseHelper(application, 1);
        }

        public Task<bool> InitPlayersAsync()
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                _players = new PlayerList();
                if (!File.Exists(PlayersFile)) return false;
                try
                {
                    using (var playersFileStream = File.Open(PlayersFile, FileMode.Open))
                    {
                        var bin = new BinaryFormatter();
                        var players = bin.Deserialize(playersFileStream) as IList<PlayerModel>;
                        if (players == null)
                        {
                            File.Delete(PlayersFile);
                            return false;
                        }
                        foreach (var player in players.Select(playerModel => new Player(playerModel)))
                        {
                            player.LoadBitmap(
                                (int)
                                    Application.Context.Resources.GetDimension(
                                        Resource.Dimension.AddPlayerItem_imageSize));
                            _players.Add(player);
                        }
                    }
                }
                catch (Exception ex)
                {
                    GaService.TrackAppException(this.ToString(), "InitPlayersAsync", ex, false);
                    File.Delete(PlayersFile);
                    return false;
                }
                return true;
            });
        }

        private string PlayersFile
        {
            get
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                return Path.Combine(documentsPath, "players.bin");
            }
        }

        public void SavePlayers()
        {
            try
            {
                using (var playersFileStream = File.Create(PlayersFile))
                {
                    var bin = new BinaryFormatter();
                    var list = _players.Items.Any()
                        ? _players.Items.Select(player => player.GetModel()).ToList()
                        : new List<PlayerModel>();
                    bin.Serialize(playersFileStream, list);
                }
            }
            catch (Exception exception)
            {
                GaService.TrackAppException("Rep", "SavePlayers", exception, false);
            }
            
        }

        public string DataBaseFile
        {
            get
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                return Path.Combine(documentsPath, "Ersh.db");
            }
        }

        public string OldDataBaseFile
        {
            get
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                return Path.Combine(documentsPath, "Yoursh.db");
            }
        }

        public PlayerList Players
        {
            get
            {
                return _players;
            }
        }

        public async Task ClearAsync()
        {
            try
            {
                _players.Reset();
                _helper.ClearAsync();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException("Rep", "ClearAsync", exception, false);
            }
        }

    }
}
