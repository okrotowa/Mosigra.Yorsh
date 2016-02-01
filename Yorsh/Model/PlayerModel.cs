using System;

namespace Yorsh.Model
{
    [Serializable]
	public class PlayerModel
    {

		public PlayerModel() : this(string.Empty, string.Empty)
		{
			
		}

        public PlayerModel(string name, string photoPath, bool isPlay = true, int score = 0)
        {
            Name = name;
			PhotoPath = photoPath;
            IsPlay = isPlay;
            Score = score;
        }

        public string PhotoPath { get; set; }

		public bool FromContacts { get; set; }

        public string Name { get; set; }

		public bool IsPlay { get; set; }

        public int Score { get; set; }

    }
}
