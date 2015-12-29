using System;
using Android.Graphics;
using Yorsh.Helpers;

namespace Yorsh.Model
{
    [Serializable]
    public class Player
    {
        public Player(string name, Bitmap photo, bool isPlay = true, int score = 0)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("Name of Player");
            if (photo == null) throw new ArgumentNullException("PlayerPhoto of Player");
            Name = name;
            Photo = photo.ToByteArray();
            IsPlay = isPlay;
            Score = score;
        }

        public Player(string name, byte[] photo, bool isPlay = true, int score = 0)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("Name of Player");
            if (photo == null) throw new ArgumentNullException("PlayerPhoto of Player");
            Name = name;
            Photo = photo;
            IsPlay = isPlay;
            Score = score;
        }

        public byte[] Photo { get; private set; }

        public string Name { get; private set; }

        public bool IsPlay { get; set; }

        public int Score { get; set; }

    }
}
