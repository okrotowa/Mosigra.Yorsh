using Android.Graphics;
using Android.Widget;
using Yorsh.Data;
using Yorsh.Model;
using Android.Views;
using Yorsh.Helpers;
using Yorsh.Activities;

namespace Yorsh.Adapters
{
	public class PlayerListAdapter : BaseAdapter<Player>
	{
		ChoosePlayerActivity _context;
		readonly PlayerList _players;
	    private readonly Color _grayColor;
	    private readonly Color _blackColor;
		public PlayerListAdapter (ChoosePlayerActivity context)
		{
			_context = context;
			_players = Rep.Instance.Players;
            _grayColor = _context.Resources.GetColor(Resource.Color.position_gray);
		    _blackColor = _context.Resources.GetColor(Resource.Color.name_black);
		}

		public override long GetItemId (int position)
		{
			return position;
		}
		private View _selectedView;
		public View SelectedView 
        {
			get { return _selectedView;}
		    private set 
            {
				if (_selectedView == value) return;
                if (_selectedView != null)
                {
                    _selectedView.FindViewById<ImageView> (Resource.Id.choosePlayer).Visibility = ViewStates.Gone;
                    _selectedView.FindViewById<TextView>(Resource.Id.playerName).SetTextColor(_grayColor);
                }
				_selectedView = value;
				_selectedView.FindViewById<ImageView> (Resource.Id.choosePlayer).Visibility = ViewStates.Visible;
                _selectedView.FindViewById<TextView>(Resource.Id.playerName).SetTextColor(_blackColor);
				_context.SetTextButtonEnabled (true);
			}
		}

		public int SelectedPosition
        {
			get;
			private set;
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			if (convertView != null) return convertView;
			var player = _players [position];
			convertView = _context.LayoutInflater.Inflate (Resource.Layout.ChoosePlayerItem, null);
			var playerImage = convertView.FindViewById<ImageView> (Resource.Id.playerImage);
			playerImage.SetImageBitmap (player.Image);
			var doneImage = convertView.FindViewById<ImageView> (Resource.Id.choosePlayer);
			doneImage.Visibility = convertView.Selected ? ViewStates.Visible : ViewStates.Gone;
			var playerName = convertView.FindViewById<TextView> (Resource.Id.playerName);
            playerName.SetTextColor(_grayColor);
			playerName.Text = player.Name;
			playerName.SetTypeface (_context.MyriadProFont (MyriadPro.Bold), Android.Graphics.TypefaceStyle.Normal);
			convertView.Click += (sender, e) => {
				SelectedView = convertView;
				SelectedPosition = position;
			};
			return convertView;
		}
	
		public override int Count 
		{
			get 
			{
				return _players.Count;
			}
		}

		public override Player this [int index] 
		{
			get 
			{
				return _players [index];
			}
		}
	}
}

