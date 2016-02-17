using System;

namespace Yorsh.Model.EventAgruments
{
    public class ScoreChangedEventArgs : EventArgs
    {
        public ScoreChangedEventArgs(int score)
        {
            Score = score;
        }
        public int Score { get; private set; }
    }
}