using System;

namespace Yorsh.Model.EventAgruments
{
    public class PlayerPositionChangedEventArgs : EventArgs
    {
        public PlayerPositionChangedEventArgs(int position)
        {
            Position = position;
        }

        public int Position { get; private set; }
    }
}