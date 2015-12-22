using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Yorsh.Helpers;
using Yorsh.Model;
using Yorsh.Fragments;
using Android.Graphics;
using System.IO;

namespace Yorsh.Activities
{
	[Activity(Label = "@string/ResultsString",ParentActivity = typeof(GameActivity), MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait)]
    public class ResultsGameActivity : BaseActivity
    {
		Bitmap bmp;

		protected  override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.ResultsGame);
			var isEndGame = Intent.GetBooleanExtra("isEnd", false);
			var listView = FindViewById<ListView>(Resource.Id.playerTournamentListView);
			var sortedPlayersList = Rep.Instance.Players.OrderByDescending (player => player.Score).ToList();
			SetFirstItem(sortedPlayersList.First(),isEndGame);
			sortedPlayersList.RemoveAt (0);
			var adapter = new ListAdapter(this, sortedPlayersList);
			listView.Adapter = adapter;

			if (isEndGame)
				SetButtonsAndActionBarIsEndGame ();
			else
				SetButtonsAndActionBarIsNotEndGame ();
		}

		void SetFirstItem(Player player, bool isEndGame)
		{
			var leadString = FindViewById<TextView> (Resource.Id.leadText);
			var imageView = FindViewById<ImageView>(Resource.Id.playerImage);
			var textScore = FindViewById<TextView> (Resource.Id.scoreText);
			var playerName = FindViewById<TextView>(Resource.Id.playerName);
			var playerScore = FindViewById<TextView>(Resource.Id.playerScore);

			leadString.SetTypeface (this.MyriadProFont (MyriadPro.BoldCondensed), TypefaceStyle.Normal);
			textScore.SetTypeface (this.MyriadProFont (MyriadPro.BoldCondensed), TypefaceStyle.Normal);
			playerName.SetTypeface (this.MyriadProFont (MyriadPro.BoldCondensed), TypefaceStyle.Normal);
			playerScore.SetTypeface (this.MyriadProFont (MyriadPro.BoldCondensed), TypefaceStyle.Normal);

			imageView.SetImageBitmap(player.Photo);
			playerName.Text = player.Name.ToUpper();
			playerScore.Text = player.Score.ToString();
			leadString.Text = Resources.GetString(isEndGame 
				? Resource.String.WinnerString 
				: Resource.String.LeadString);
			
		}

		void SetButtonsAndActionBarIsNotEndGame()
		{
			var startPlayButton = FindViewById<Button>(Resource.Id.endGameButton);
			startPlayButton.Visibility = ViewStates.Visible;
			FindViewById<RelativeLayout> (Resource.Id.relativeLayout).Visibility = ViewStates.Gone;
			this.ActionBar.Show ();
			startPlayButton.Touch+=(sender, e)=>this.OnTouchButtonDarker(startPlayButton, e);
			startPlayButton.SetTypeface (this.MyriadProFont (MyriadPro.BoldCondensed), TypefaceStyle.Normal);
			startPlayButton.Click+= delegate {
				
				var preferences = GetPreferences(FileCreationMode.Private);
				var isShow = preferences.GetBoolean("isShow", true);
				if (isShow)
				{
					var fragmentTransaction = FragmentManager.BeginTransaction();
					var prev = (DialogFragment) FragmentManager.FindFragmentByTag("rating");
					var dialog = prev ?? new DialogRatingFragment() {ShowsDialog = true};
					dialog.Show(fragmentTransaction, "rating");
				}
				else
				{
					Intent.PutExtra("isEnd",true);
					this.Recreate();
				}
			};
		}

		void SetButtonsAndActionBarIsEndGame()
		{
			FindViewById<Button> (Resource.Id.endGameButton).Visibility = ViewStates.Gone;
			FindViewById<RelativeLayout> (Resource.Id.relativeLayout).Visibility = ViewStates.Visible;
			this.ActionBar.Hide();

			var startNewGameButton = FindViewById<Button> (Resource.Id.startNewGameButton);
		    startNewGameButton.Enabled = true;
            startNewGameButton.Click += delegate
            {
                Rep.Instance.Clear();
                this.StartActivityWithoutBackStack(new Intent(this, typeof(MainMenuActivity)));
            };
			startNewGameButton.Touch+=(sender, e)=>this.OnTouchButtonDarker(startNewGameButton, e);
			startNewGameButton.SetTypeface (this.MyriadProFont (MyriadPro.BoldCondensed), TypefaceStyle.Normal);
            startNewGameButton.Background.SetAlpha(255);

			var shareButton = FindViewById<Button> (Resource.Id.shareButton);
			shareButton.Touch+=(sender, e)=>this.OnTouchButtonDarker(shareButton, e);
			shareButton.SetTypeface (this.MyriadProFont (MyriadPro.BoldCondensed), TypefaceStyle.Normal);
			shareButton.Click += delegate
			{
				//twitter
				var contentview = FindViewById (Resource.Id.tournament);
				contentview.BuildDrawingCache();
				bmp = contentview.DrawingCache;
				var sdCardPath = Environment.ExternalStorageDirectory.AbsolutePath;
				var filePath = System.IO.Path.Combine(sdCardPath, "test.png");
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					bmp.Compress(Bitmap.CompressFormat.Png, 100, stream);
				}

				var fragmentTrans = FragmentManager.BeginTransaction();
				var prev = (DialogFragment)FragmentManager.FindFragmentByTag("share");
				var dialogShare = prev ?? new ShareFragment() { ShowsDialog = true };
				dialogShare.Show(fragmentTrans, "share");
			};
		}

		private class ListAdapter : BaseAdapter<Player>
		{
			private readonly Activity _context;
			private readonly IList<Player> _players;

			public ListAdapter(Activity context, IList<Player> players)
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
				return convertView ?? SetPlayerView (position);
			}

			private View SetPlayerView(int position)
			{
				var inflater = (LayoutInflater)_context.GetSystemService(LayoutInflaterService);
				var view = inflater.Inflate(Resource.Layout.PlayerItem, null);

				var imageView = view.FindViewById<ImageView>(Resource.Id.playerImage);
				var playerName = view.FindViewById<TextView>(Resource.Id.playerName);
				var playerScore = view.FindViewById<TextView>(Resource.Id.playerScore);
				playerName.SetTypeface (_context.MyriadProFont (MyriadPro.BoldCondensed), TypefaceStyle.Normal);
				playerScore.SetTypeface (_context.MyriadProFont (MyriadPro.BoldCondensed), TypefaceStyle.Normal);
				imageView.SetImageBitmap(this[position].Photo);
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


}