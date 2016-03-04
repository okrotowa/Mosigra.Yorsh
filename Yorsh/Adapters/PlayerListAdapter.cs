using Android.Graphics;
using Android.Widget;
using Yorsh.Data;
using Yorsh.Model;
using Android.Views;
using Yorsh.Activities;

namespace Yorsh.Adapters
{
	public class PlayerListAdapter : BaseAdapter<Player>
	{
	    readonly ChoosePlayerActivity _context;
		readonly PlayerList _players;
	    private readonly Color _grayColor;
	    private readonly Color _blackColor;

        private View _selectedView;

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
            var view = _context.LayoutInflater.Inflate(Resource.Layout.ChoosePlayerItem, null);
            var playerImage = view.FindViewById<ImageView>(Resource.Id.playerImage);
            var doneImage = view.FindViewById<ImageView>(Resource.Id.choosePlayer);
            var playerName = view.FindViewById<TextView>(Resource.Id.playerName);
            playerName.SetTextColor(_grayColor);
            playerName.SetTypeface(Rep.FontManager.Get(Font.Bold), TypefaceStyle.Normal);
			var player = _players [position];
            playerImage.SetImageBitmap (player.Image);
			doneImage.Visibility = view.Selected ? ViewStates.Visible : ViewStates.Gone;
			playerName.Text = player.Name;
			view.Click += (sender, e) => 
            {
				SelectedView = view;
				SelectedPosition = position;
			};
			return view;
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

