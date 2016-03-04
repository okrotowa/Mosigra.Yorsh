using System;
using System.Collections;
using System.Collections.Generic;
using Yorsh.Model.EventAgruments;


namespace Yorsh.Model
{
    public class PlayerEnumerator : IEnumerator<Player>
    {
        private PlayerList _playersList;
        private int _current;
        public event EventHandler<PlayerPositionChangedEventArgs> CurrentPositionChanged;
        public PlayerEnumerator(PlayerList playersList)
        {
            _playersList = playersList;
            _current = 0;
        }

        public bool MoveNext()
        {
            CurrentPosition = CurrentPosition >= _playersList.Count - 1 ? 0 : CurrentPosition + 1;
            return true;
        }

        public void Reset()
        {
            CurrentPosition = 0;
        }

        public void SetCurrent(int position)
        {
            var count = _playersList.Count;
            CurrentPosition = position > count ? count : position;
        }

        public Player Current
        {
            get
            {
                return _playersList[CurrentPosition];
            }
        }

        public int CurrentPosition
        {
            get { return _current; }
            private set
            {
                _current = value;
                if (CurrentPositionChanged!=null) 
                    CurrentPositionChanged.Invoke(this, new PlayerPositionChangedEventArgs(_current));
            }
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