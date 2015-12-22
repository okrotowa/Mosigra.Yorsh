using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Yorsh.Fragments;
using Yorsh.Helpers;
using Yorsh.Model;

namespace Yorsh.Activities
{
	[Activity(Label = "@string/GameString", MainLauncher = false, ParentActivity = typeof(MainMenuActivity), ScreenOrientation = ScreenOrientation.Portrait)]
    public class GameActivity : BaseActivity
    {
        bool _isRefusing = false;
		TextView _playerName;
		TextView _playerPosition;
		Button _makeThisButton;
		Button _refuseButton;
		ImageButton _cardImage;
		TextView _actionButton;
		TaskEnumerator _taskEnumerator;
		TextView _playerScore;
		TextView _points;
		TextView _x2;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			//await StubInitialize();
            SetContentView(Resource.Layout.StartGame);
			SetHomeButtonEnabled (false);
			Initialize ();

			_cardImage.Click += (sender, args) =>
            {
				var intent = new Intent(this, typeof(ImageActivity));
				intent.PutExtra("taskId", _taskEnumerator.Current.Id);
				StartActivityForResult(intent, 100);
				SetButtonsEnabled(true);
            };

			_makeThisButton.Touch += (sender, args) => this.OnTouchButtonDarker (_makeThisButton, args);
            _makeThisButton.Click += delegate
            {
                Rep.Instance.Players.Current.Score += _taskEnumerator.Current.Score;
				ShowDialogOnButtonClick(TaskDialog.Make);
            };

			_refuseButton.Touch += (sender, args) => this.OnTouchButtonDarker (_refuseButton, args);
            _refuseButton.Click += delegate
            {
                if (!_isRefusing) _taskEnumerator.Current.Score *= 2;
				ShowDialogOnButtonClick(_isRefusing ? TaskDialog.RefuseAndMove : TaskDialog.Refuse);
            };

			_actionButton.Click+= delegate 
			{
				var intent = new Intent(this, typeof (ResultsGameActivity));
				intent.PutExtra("isEnd", false);
				StartActivity(intent);
			};

			_taskEnumerator.MoveNext();
			MoveNextStep ();
        }
        
	    private void Initialize()
		{			
			_playerName = FindViewById<TextView> (Resource.Id.playerInGameName);
			_playerPosition = FindViewById<TextView> (Resource.Id.playerInGamePostion);
			_playerScore = FindViewById<TextView> (Resource.Id.playerInGameScore); 
			_makeThisButton = FindViewById<Button>(Resource.Id.makeThisButton);
			_refuseButton = FindViewById<Button>(Resource.Id.refuseButton);
			_cardImage = FindViewById<ImageButton>(Resource.Id.taskImageButton);
			_actionButton = (TextView)CreateActionButton (Resource.Drawable.table_button);
			_points = FindViewById<TextView> (Resource.Id.points);
			_x2 = FindViewById<TextView> (Resource.Id.x2);
			_taskEnumerator = (TaskEnumerator)Rep.Instance.Tasks.GetEnumerator ();		

			_playerName.SetTypeface(this.MyriadProFont(MyriadPro.Bold),TypefaceStyle.Normal);
			_playerPosition.SetTypeface(this.MyriadProFont(MyriadPro.Regular),TypefaceStyle.Normal);
			_makeThisButton.SetTypeface(this.MyriadProFont(MyriadPro.BoldCondensed), TypefaceStyle.Normal);
			_refuseButton.SetTypeface(this.MyriadProFont(MyriadPro.BoldCondensed), TypefaceStyle.Normal);
			_playerScore.SetTypeface(this.MyriadProFont(MyriadPro.BoldCondensed), TypefaceStyle.Normal);
			_points.SetTypeface (this.MyriadProFont (MyriadPro.Bold), TypefaceStyle.Normal);
			_x2.SetTypeface (this.MyriadProFont (MyriadPro.Bold), TypefaceStyle.Normal);
			_cardImage.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.card_backside));
			FindViewById<TextView>(Resource.Id.scoreString).SetTypeface(this.MyriadProFont(MyriadPro.Condensed), TypefaceStyle.Normal);
		}

		private void SetButtonsEnabled(bool enabled)
		{
		    _refuseButton.Enabled = enabled;
            _refuseButton.Background.SetAlpha(enabled ? 255 : 50);
            _refuseButton.SetTextColor(Resources.GetColor(enabled
                ? Resource.Color.white
                : Resource.Color.pressed_text_color));

		    _makeThisButton.Enabled = enabled;
            _makeThisButton.Background.SetAlpha(enabled ? 255 : 50);
            _makeThisButton.SetTextColor(_makeThisButton.Resources.GetColor(enabled
                ? Resource.Color.white
                : Resource.Color.pressed_text_color));
		}


		public void ShowDialogOnButtonClick(TaskDialog taskDialog)
		{
		   	ShowDialogOnButtonClick (taskDialog, Rep.Instance.Players.Current);
		}

		public void ShowDialogOnButtonClick(TaskDialog taskDialog, Player currentPlayer)
		{
			var task = ShowDialogTask (taskDialog, currentPlayer);
			task.RunSynchronously();
		}

		private Task ShowDialogTask(TaskDialog taskDialog, Player currentPlayer)
		{
			var fragmentTransaction = FragmentManager.BeginTransaction();
			var prev = FragmentManager.FindFragmentByTag("answer");
			if (prev != null) fragmentTransaction.Remove(prev);
			var taskScore = Rep.Instance.Tasks.Current.Score;
			var dialog = new TaskProgressDialog (taskDialog, currentPlayer, taskScore) { ShowsDialog = true };
			dialog.Show (fragmentTransaction, "answer");

			_isRefusing = taskDialog == TaskDialog.Refuse;
			return !_isRefusing && !_taskEnumerator.MoveNext () 
				? new Task (EndOfGame) 
				: new Task (MoveNextStep);
		}

		void MoveNextStep ()
        {
			_x2.Visibility = _isRefusing ? ViewStates.Visible : ViewStates.Invisible;
			_points.Text = (_isRefusing ? _taskEnumerator.Current.Score / 2 
				: _taskEnumerator.Current.Score==0 
					? new Random().Next(3) + 1
					: _taskEnumerator.Current.Score)
				.ToString();
			Rep.Instance.Players.MoveNext ();
            var player = Rep.Instance.Players.Current;
            FindViewById<ImageView>(Resource.Id.playerInGameImage).SetImageBitmap(player.Photo);
			_playerName.Text = player.Name;
			_playerPosition.Text = Resources.GetString(Resource.String.PositionPlayerString) + Rep.Instance.Players.GetPosition(player);
			_playerScore.Text= player.Score.ToString();
			SetButtonsEnabled (false);
        }

		void EndOfGame()
		{
			GetShopDialog ().Show ();
            //var intent = new Intent (this, typeof(ResultsGameActivity));
            //intent.PutExtra ("isEnd", true);
            //this.StartActivityWithoutBackStack (intent);
		}

		private AlertDialog.Builder GetShopDialog ()
		{
			var builder = new AlertDialog.Builder (this);
			builder.SetTitle (Resource.String.GoToShopingString);
			builder.SetMessage (Resource.String.GoToSaleString);
			builder.SetPositiveButton (GetString (Resource.String.YesString), delegate {
				//this.StartActivity(typeof(StoreActivity));
			                                                                               this.AddProduct("10_task");
			});
			builder.SetNegativeButton (GetString (Resource.String.NoString), delegate {
				var intent = new Intent (this, typeof(ResultsGameActivity));
				intent.PutExtra ("isEnd", true);
				this.StartActivityWithoutBackStack (intent);
			});
		    builder.SetCancelable(false);
			return builder;
		}

        public override void OnBackPressed()
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetMessage(GetString(Resource.String.AreYouShureString));
            builder.SetCancelable(true);
            builder.SetPositiveButton(GetString(Resource.String.YesString), (sender, args) =>
            {
                Rep.Instance.Clear();
                this.StartActivityWithoutBackStack(new Intent(this, typeof(MainMenuActivity)));
            });
            builder.SetNegativeButton(GetString(Resource.String.NoString), (sender, args) =>
            {

            });

            var alertDialog = builder.Create();
            alertDialog.Show();
        }

	    protected override void OnDestroy()
	    {
            SetButtonsEnabled(true);
	        base.OnDestroy();
	    }

	    /// <exception cref="T:System.InvalidOperationException"></exception>
		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (resultCode == Result.Canceled) return;
			if (requestCode ==100)
				StartActivityForResult(typeof(ChoosePlayerActivity), 101);
			if (requestCode ==101)
			{	
				var position = data.GetIntExtra("player_id", -1);
				if (position < 0)
					throw new InvalidOperationException ("You should choose player");
				var player = Rep.Instance.Players [position];
				player.Score -= 5;
				ShowDialogOnButtonClick (TaskDialog.Bear, player);
			}
        }

    }
}