using Android.Widget;
using Yorsh.Data;
using Yorsh.Listeners;
using Yorsh.Model;
using Android.Views;
using Yorsh.Activities;

namespace Yorsh.Adapters
{
	public class AddNewPlayerListAdapter : BaseAdapter<Player>
	{
		private readonly AddPlayersActivity _context;
		private readonly PlayerList _players;

        public AddNewPlayerListAdapter(AddPlayersActivity context)
		{
			_context = context;
			_players = Rep.Instance.Players;
	        Initialize();
		}

	    private void Initialize()
	    {

	    }

	    public override long GetItemId(int position)
		{
			return position;
		}
       
        public override View GetView(int position, View convertView, ViewGroup parent)
		{
			if (convertView != null) return convertView;
            var inflater = _context.LayoutInflater;
            var view = inflater.Inflate(Resource.Layout.AddPlayerItem, null);
            var playerImage = view.FindViewById<ImageView>(Resource.Id.playerImage);
            var playerName = view.FindViewById<TextView>(Resource.Id.playerName);
            playerName.SetTypeface(Rep.FontManager.Get(Font.Bold), Android.Graphics.TypefaceStyle.Normal);
            var enableTextView = view.FindViewById<TextView>(Resource.Id.isPlayText);
            enableTextView.SetTypeface(Rep.FontManager.Get(Font.Regular), Android.Graphics.TypefaceStyle.Normal);
            var removeButton = view.FindViewById<ImageButton>(Resource.Id.removeButton);
            var doneImage = view.FindViewById<ImageView>(Resource.Id.doneImage);
            var playerNameLayout = view.FindViewById<RelativeLayout>(Resource.Id.playerNameLayout);

			var player = _players[position];
            playerImage.SetImageBitmap(player.Image);
			playerName.Text = player.Name;
			enableTextView.Enabled = player.IsPlay;

			if (player.IsPlay)
			{
                removeButton.Visibility = ViewStates.Gone;
				doneImage.Visibility = ViewStates.Visible;
				enableTextView.Text = _context.Resources.GetString(Resource.String.IsPlayString);
			}
			else
			{
				removeButton.Visibility = ViewStates.Visible;
				doneImage.Visibility = ViewStates.Gone;
				enableTextView.Text = _context.Resources.GetString(Resource.String.IsNotPlayString);
			}

            removeButton.SetOnClickListener(new RemoveButtonClickListener(position));

			playerNameLayout.Click += (sender, e) =>
			{
				var isEnabledNew = !enableTextView.Enabled;
				enableTextView.Enabled = isEnabledNew;
				Rep.Instance.Players[position].IsPlay = isEnabledNew;

				if (isEnabledNew)
				{
					removeButton.Visibility = ViewStates.Gone;
					doneImage.Visibility = ViewStates.Visible;
					enableTextView.Text = _context.Resources.GetString(Resource.String.IsPlayString);
				}
				else
				{
					removeButton.Visibility = ViewStates.Visible;
					doneImage.Visibility = ViewStates.Gone;
					enableTextView.Text = _context.Resources.GetString(Resource.String.IsNotPlayString);
				}

				_context.SetButtonEnabled(_players.Count <= 1 || Rep.Instance.Players.IsAllPlay);
			};

            convertView = view;
			return convertView;
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

