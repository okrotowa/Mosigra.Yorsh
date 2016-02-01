using System.Collections;
using System.Collections.Generic;


namespace Yorsh.Model
{
    public class PlayerEnumerator : IEnumerator<Player>
    {
        private readonly PlayerList _playersList;
        private int _current;

        public PlayerEnumerator(PlayerList playersList)
        {
            _playersList = playersList;
            _current = 0;
        }

        public bool MoveNext()
        {
            if (_current >= _playersList.Count - 1) _current = 0;
            else
                _current++;
            return true;
        }

        public void Reset()
        {
            _current = 0;
        }

        public void SetCurrent(int position)
        {
            _current = position;
        }

        public Player Current
        {
            get { return _playersList[_current]; }
        }

        public int CurrentPosition
        {
            get { return _current; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public void Dispose()
        {
        }
    }
}