using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Yorsh.Model
{
    public class PlayerList : IList<Player>
    {
        private readonly PlayerEnumerator _enumerator;
        private readonly IList<Player> _players;

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
            _players.Add(item);
        }

        public int GetPosition(Player player)
        {
			return _players.All(p=>player.Score==0) ? 0 : _players.Count(p => p.Score > player.Score) + 1;
        }

        public void Reset()
        {
            foreach (var player in _players)
            {
                player.Score = 0;
            }
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
        }

        public bool Contains(Player item)
        {
            return _players.Contains(item);
        }

        public void CopyTo(Player[] array, int arrayIndex)
        {
            _players.CopyTo(array, arrayIndex);
        }


        public int IndexOf(Player player)
        {
            return _players.IndexOf(player);
        }

        public void Insert(int index, Player item)
        {
            _players.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _players.RemoveAt(index);
            OnItemRemoved();
        }

        public bool Remove(Player player)
        {
            OnItemRemoved();
            return _players.Remove(player);
        }

        public Player this[int index]
        {
            get { return _players[index]; }
            set { _players[index] = value; }
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