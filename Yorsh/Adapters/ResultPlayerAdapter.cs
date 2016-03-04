using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Yorsh.Data;
using Yorsh.Model;

namespace Yorsh.Adapters
{
    public class ResultPlayersAdapter : BaseAdapter<Player>
    {
        private readonly Activity _context;
        private readonly IList<Player> _players;

        public ResultPlayersAdapter(Activity context, IList<Player> players)
        {
            _context = context;
            _players = players;


        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            return convertView ?? SetPlayerView(position);
        }

        private View SetPlayerView(int position)
        {
            var inflater = _context.LayoutInflater;
            var view = inflater.Inflate(Resource.Layout.PlayerItem, null);

            var imageView = view.FindViewById<ImageView>(Resource.Id.playerImage);
            var playerName = view.FindViewById<TextView>(Resource.Id.playerName);
            var playerScore = view.FindViewById<TextView>(Resource.Id.playerScore);
            playerName.SetTypeface(Rep.FontManager.Get(Font.BoldCondensed), TypefaceStyle.Normal);
            playerScore.SetTypeface(Rep.FontManager.Get(Font.BoldCondensed), TypefaceStyle.Normal);

            imageView.SetImageBitmap(_players[position].Image);
            playerName.Text = this[position].Name;
            playerScore.Text = this[position].Score.ToString();
            return view;
        }

        public override int Count
        {
            get { return _players.Count; }
        }

        public override Player this[int position]
        {
            get { return _players[position]; }
        }


    }
}