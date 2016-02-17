using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Yorsh.Model.EventAgruments;

namespace Yorsh.Model
{
    public sealed class PlayerList : IList<Player>
    {
        private readonly PlayerEnumerator _enumerator;
        private readonly IList<Player> _players;
        public event EventHandler<ScoreChangedEventArgs> ScoreChanged;
        public event EventHandler CollectionChanged;

        private void OnCollectionChanged()
        {
            EventHandler handler = CollectionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public PlayerList()
        {
            _enumerator = new PlayerEnumerator(this);
            _players = new List<Player>();
        }

        public IEnumerable<Player> Items
        {
            get { return _players; }
        }

        public bool IsAllPlay
        {
            get
            {
                return _players.All(t => t.IsPlay);
            }
        }
        public void Add(Player item)
        {
            this.Add(item, true);
        }
        public void Add(Player item, bool raiseOnCollectionChanged)
        {
            _players.Add(item);
            if (raiseOnCollectionChanged) OnCollectionChanged();
            item.ScoreChanged += ItemOnScoreChanged;
        }
        private void ItemOnScoreChanged(object sender, ScoreChangedEventArgs e)
        {
            if (ScoreChanged != null) ScoreChanged.Invoke(this, e);
        }

        public int GetPosition(Player player)
        {
            return _players.All(p => p.Score == 0) ? 0 : _players.Count(p => p.Score > player.Score) + 1;
        }

        public void Reset()
        {
            foreach (var player in _players)
            {
                player.GetModel().Score = 0;
            }
            if (ScoreChanged != null) ScoreChanged.Invoke(this, new ScoreChangedEventArgs(0));
            _enumerator.Reset();
        }

        public event EventHandler ItemRemoved;
        private void OnItemRemoved()
        {
            var handler = ItemRemoved;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public int Count
        {
            get { return _players.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Clear()
        {
            _players.Clear();
            OnCollectionChanged();
        }

        public bool Contains(Player item)
        {
            return _players.Contains(item);
        }

        public void CopyTo(Player[] array, int arrayIndex)
        {
            _players.CopyTo(array, arrayIndex);
            OnCollectionChanged();
        }


        public int IndexOf(Player player)
        {
            return _players.IndexOf(player);
        }

        public void Insert(int index, Player item)
        {
            _players.Insert(index, item);
            OnCollectionChanged();
        }

        public void RemoveAt(int index)
        {
            if (_players.Count > index)
            {
                _players[index].ScoreChanged -= ItemOnScoreChanged;
                _players.RemoveAt(index);
                OnCollectionChanged();
                OnItemRemoved();
            }
        }

        public bool Remove(Player player)
        {
            var removedPlayer = _players.Remove(player);
            if (removedPlayer)
            {
                player.ScoreChanged -= ItemOnScoreChanged;
                OnCollectionChanged();
                OnItemRemoved();
            }

            return removedPlayer;
        }

        public Player this[int index]
        {
            get { return _players[index]; }
            set
            {
                _players[index] = value;
                OnCollectionChanged();
            }
        }

        public PlayerEnumerator Enumerator
        {
            get { return _enumerator; }
        }

        public IEnumerator<Player> GetEnumerator()
        {
            return _enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _enumerator;
        }
    }
}