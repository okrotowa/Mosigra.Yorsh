using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Yorsh.Adapters;
using Yorsh.Data;
using Yorsh.Helpers;
using Yorsh.Model;
using Yorsh.Fragments;
using Android.Graphics;

namespace Yorsh.Activities
{
    [Activity(Label = "@string/ResultsString", ParentActivity = typeof(GameActivity), MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait)]
    public class ResultsGameActivity : BaseActivity
    {
        private bool _isEndGame;
        private ListView _listView;
        private Button _endGameButton;
        private Button _startNewGameButton;
        private DialogRatingFragment _dialog;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ResultsGame);
            Initialize();
            SetUps(_isEndGame);

            if (_isEndGame) 
                this.SaveAsStartupActivity(StringConst.ResultGameActivity);

            RegisterSubscribes();
        }

        private void Initialize()
        {
            try
            {
                _isEndGame = Intent.GetBooleanExtra("isEnd", false);

                _listView = FindViewById<ListView>(Resource.Id.playerTournamentListView);
                var sortedPlayersList = Rep.Instance.Players.Players.OrderByDescending(player => player.Score).ToList();
                SetFirstItem(sortedPlayersList.First(), _isEndGame);
                sortedPlayersList.RemoveAt(0);
                var adapter = new ResultPlayersAdapter(this, sortedPlayersList);
                _listView.Adapter = adapter;

                _startNewGameButton = FindViewById<Button>(Resource.Id.startNewGameButton);
                this.AddButtonTouchListener(_startNewGameButton);
                _startNewGameButton.SetTypeface(Rep.FontManager.Get(Font.BoldCondensed), TypefaceStyle.Normal);

                _endGameButton = FindViewById<Button>(Resource.Id.endGameButton);
                this.AddButtonTouchListener(_endGameButton);
                _endGameButton.SetTypeface(Rep.FontManager.Get(Font.BoldCondensed), TypefaceStyle.Normal);

                _dialog = new DialogRatingFragment() { ShowsDialog = true };
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "Initialize", exception, false);
            }

        }
        private void DialogOnDissmissed(object sender, EventArgs eventArgs)
        {
            this.Recreate();
        }
        protected override void RegisterSubscribes()
        {
            _endGameButton.Click += StartPlayButtonOnClick;
            _startNewGameButton.Click += StartNewGameButtonOnClick;
            _dialog.Dissmissed += DialogOnDissmissed;
        }
        protected override void UnregisterSubscribes()
        {
            _endGameButton.Click -= StartPlayButtonOnClick;
            _startNewGameButton.Click -= StartNewGameButtonOnClick;
            _dialog.Dissmissed -= DialogOnDissmissed;

        }
        private async void StartNewGameButtonOnClick(object sender, EventArgs eventArgs)
        {
            await this.StartNewGameAsync();
        }
        
        void SetFirstItem(Player player, bool isEndGame)
        {
            try
            {
                var leadString = FindViewById<TextView>(Resource.Id.leadText);
                var imageView = FindViewById<ImageView>(Resource.Id.playerImage);
                var textScore = FindViewById<TextView>(Resource.Id.scoreText);
                var playerName = FindViewById<TextView>(Resource.Id.playerName);
                var playerScore = FindViewById<TextView>(Resource.Id.playerScore);

                leadString.SetTypeface(Rep.FontManager.Get(Font.BoldCondensed), TypefaceStyle.Normal);
                textScore.SetTypeface(Rep.FontManager.Get(Font.BoldCondensed), TypefaceStyle.Normal);
                playerName.SetTypeface(Rep.FontManager.Get(Font.BoldCondensed), TypefaceStyle.Normal);
                playerScore.SetTypeface(Rep.FontManager.Get(Font.BoldCondensed), TypefaceStyle.Normal);

                imageView.SetImageBitmap(player.Image);
                playerName.Text = player.Name.ToUpper();
                playerScore.Text = player.Score.ToString();
                leadString.Text = Resources.GetString(isEndGame
                    ? Resource.String.WinnerString
                    : Resource.String.LeadString);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "SetFirstItem", exception, false);
            }
           

        }

        public async override void OnBackPressed()
        {
            if (_isEndGame) await StartNewGameAsync();
            else base.OnBackPressed();
        }
        
        private void StartPlayButtonOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var preferences = GetPreferences(FileCreationMode.Private);
                var isShow = preferences.GetBoolean("isShow", true);
                if (isShow)
                {
                    var fragmentTransaction = FragmentManager.BeginTransaction();
                    var prev = (DialogFragment)FragmentManager.FindFragmentByTag("rating");
                    var dialog = (DialogRatingFragment)prev ?? _dialog;
                    dialog.Show(fragmentTransaction, "rating");
                }
                else
                {
                    this.Recreate();
                }
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "StartPlayButtonOnClick", exception, false);
            }
            
        }

        public override void Recreate()
        {
            Intent.PutExtra("isEnd", true);
            base.Recreate();
        }


        private async Task StartNewGameAsync()
        {
            var intent = new Intent(this, typeof (MainMenuActivity));
            this.StartActivityWithoutBackStack(intent);
        }
        private void SetUps(bool isEnd)
        {
            _endGameButton.Visibility = isEnd ? ViewStates.Gone : ViewStates.Visible;
            _endGameButton.Enabled = !isEnd;
            _startNewGameButton.Visibility = isEnd ? ViewStates.Visible : ViewStates.Gone;
            _startNewGameButton.Enabled = isEnd;
            if (isEnd) ActionBar.Hide();
            else ActionBar.Show();

            #region sharing

            //startNewGameButton.Touch += (sender, e) => this.OnTouchButtonDarker(startNewGameButton, e);


            //            var shareButton = FindViewById<Button>(Resource.Id.shareButton);
            //            shareButton.Touch += (sender, e) => this.OnTouchButtonDarker(shareButton, e);
            //            shareButton.SetTypeface(Rep.FontManager.Get(Font.BoldCondensed), TypefaceStyle.Normal);
            //            shareButton.Click += delegate
            //            {
            ////                var contentview = FindViewById(Resource.Id.tournament);
            ////                contentview.BuildDrawingCache();
            ////                _screenShot = contentview.DrawingCache;
            ////                var sdCardPath = Environment.ExternalStorageDirectory.AbsolutePath;
            ////                var filePath = System.IO.Path.Combine(sdCardPath, "test.png");
            ////                using (var stream = new FileStream(filePath, FileMode.Create))
            ////                {
            ////                    _screenShot.Compress(Bitmap.CompressFormat.Png, 100, stream);
            ////                }
            ////
            ////                var fragmentTrans = FragmentManager.BeginTransaction();
            ////                var prev = (DialogFragment)FragmentManager.FindFragmentByTag("share");
            ////                var dialogShare = prev ?? new ShareFragment() { ShowsDialog = true };
            ////                dialogShare.Show(fragmentTrans, "share");
            //            };

            #endregion


        }


    }


}