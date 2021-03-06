using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Yorsh.Data;
using Yorsh.Helpers;
using Yorsh.Model;
using Yorsh.Model.EventAgruments;

namespace Yorsh.Activities
{
    [Activity(Label = "@string/GameString", ParentActivity = typeof(MainMenuActivity), ScreenOrientation = ScreenOrientation.Portrait)]
    public class GameActivity : BaseActivity
    {

        #region Initialization

        TextView _playerName;
        TextView _playerPosition;
        Button _makeThisButton;
        Button _refuseButton;
        ImageButton _cardImage;
        TextView _actionButton;
        PlayerEnumerator _playerEnumerator;
        TaskEnumerator _taskEnumerator;
        TextView _playerScore;
        TextView _points;
        TextView _x2;
        TaskDialog _taskDialog;
        private Color _greenColor;
        private Color _redColor;
        private ImageButton _coninueButton;
        private TextView _scoreTextView;
        private TextView _statusTitleText;
        private TextView _statusDescriptionText;
        private TextView _changeCountScoreText;
        private TextView _endDescriptionText;
        private TextView _currentScoreText;
        private RelativeLayout _taskProgressView;

        private void Initialize()
        {
            try
            {
                _redColor = Resources.GetColor(Resource.Color.task_progress_red);
                _greenColor = Resources.GetColor(Resource.Color.task_progress_green);

                _playerName = FindViewById<TextView>(Resource.Id.playerInGameName);
                _playerPosition = FindViewById<TextView>(Resource.Id.playerInGamePostion);
                _playerScore = FindViewById<TextView>(Resource.Id.playerInGameScore);
                _makeThisButton = FindViewById<Button>(Resource.Id.makeThisButton);
                _refuseButton = FindViewById<Button>(Resource.Id.refuseButton);
                _cardImage = FindViewById<ImageButton>(Resource.Id.taskImageButton);
                _actionButton = (TextView)CreateActionButton(Resource.Drawable.table_button);
                _points = FindViewById<TextView>(Resource.Id.points);
                _x2 = FindViewById<TextView>(Resource.Id.x2);
                _coninueButton = FindViewById<ImageButton>(Resource.Id.continueButton);
                _scoreTextView = FindViewById<TextView>(Resource.Id.scoreString);

                _taskEnumerator = Rep.DatabaseHelper.Tasks.Enumerator;
                _playerEnumerator = Rep.Instance.Players.Enumerator;

                _playerName.SetTypeface(Rep.FontManager.Get(Font.Bold), TypefaceStyle.Normal);
                _playerPosition.SetTypeface(Rep.FontManager.Get(Font.Regular), TypefaceStyle.Normal);
                _makeThisButton.SetTypeface(Rep.FontManager.Get(Font.BoldCondensed), TypefaceStyle.Normal);
                _refuseButton.SetTypeface(Rep.FontManager.Get(Font.BoldCondensed), TypefaceStyle.Normal);
                _playerScore.SetTypeface(Rep.FontManager.Get(Font.BoldCondensed), TypefaceStyle.Normal);
                _points.SetTypeface(Rep.FontManager.Get(Font.Bold), TypefaceStyle.Normal);
                _x2.SetTypeface(Rep.FontManager.Get(Font.Bold), TypefaceStyle.Normal);
                _cardImage.SetImageResource(Resource.Drawable.card_backside); ;
                _scoreTextView.SetTypeface(Rep.FontManager.Get(Font.Condensed), TypefaceStyle.Normal);

                _taskProgressView = FindViewById<RelativeLayout>(Resource.Id.taskProgressView);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "Initialize", exception, true);
            }
        }
        private void InitializeTaskProgressBox()
        {

            _statusTitleText = FindViewById<TextView>(Resource.Id.statusTitleText);
            _statusTitleText.SetTypeface(Rep.FontManager.Get(Font.SemiboldCondensed), TypefaceStyle.Normal);

            _statusDescriptionText = FindViewById<TextView>(Resource.Id.statusDescriptionText);
            _statusDescriptionText.SetTypeface(Rep.FontManager.Get(Font.Condensed), TypefaceStyle.Normal);

            var boldConsededFont = Rep.FontManager.Get(Font.BoldCondensed);
            _changeCountScoreText = FindViewById<TextView>(Resource.Id.changeCountScoreText);
            _changeCountScoreText.SetTypeface(boldConsededFont, TypefaceStyle.Normal);

            _endDescriptionText = FindViewById<TextView>(Resource.Id.endDescriptionText);
            _endDescriptionText.SetTypeface(Rep.FontManager.Get(Font.LightCondensed), TypefaceStyle.Normal);

            _currentScoreText = FindViewById<TextView>(Resource.Id.currentScoreText);
            _currentScoreText.SetTypeface(boldConsededFont, TypefaceStyle.Normal);
        }

        protected override void RegisterSubscribes()
        {
            AddButtonTouchListener(_makeThisButton);
            AddButtonTouchListener(_refuseButton);

            Rep.Instance.Players.ScoreChanged += PlayersOnScoreChanged;
            _playerEnumerator.CurrentPositionChanged += EnumeratorOnCurrentPositionChanged;

            _coninueButton.Click += ConinueButtonOnClick;
            _cardImage.Click += CardImageOnClick;
            _makeThisButton.Click += MakeThisButtonOnClick;
            
            _refuseButton.Click += RefuseButtonOnClick;
            _actionButton.Click += ActionButtonOnClick;
        }

        protected override void UnregisterSubscribes()
        {
            Rep.Instance.Players.ScoreChanged -= PlayersOnScoreChanged;
            _playerEnumerator.CurrentPositionChanged -= EnumeratorOnCurrentPositionChanged;

            _coninueButton.Click -= ConinueButtonOnClick;
            _cardImage.Click -= CardImageOnClick;
            _makeThisButton.Click -= MakeThisButtonOnClick;

            _refuseButton.Click -= RefuseButtonOnClick;
            _actionButton.Click -= ActionButtonOnClick;
        }
        #endregion
        #region ShopDialog

        private AlertDialog.Builder GetShopDialog()
        {
            try
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetTitle(Resource.String.GoToShopingString);
                builder.SetMessage(Resource.String.GoToSaleString);
                builder.SetPositiveButton(GetString(Resource.String.YesString), delegate
                {
                    var store = new Intent(this, typeof(StoreActivity));
                    store.PutExtra("from_game", true);
                    this.StartActivityWithoutBackStack(store);
                });
                builder.SetNegativeButton(GetString(Resource.String.NoString), delegate
                {
                    var intent = new Intent(this, typeof(ResultsGameActivity));
                    intent.PutExtra("isEnd", true);
                    this.StartActivityWithoutBackStack(intent);
                });
                builder.SetCancelable(false);
                return builder;
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "GetShopDialog", exception, false);
                throw;
            }

        }

        #endregion
        #region IndependentSubsribes

        private void ActionButtonOnClick(object sender, EventArgs eventArgs)
        {
            var intent = new Intent(this, typeof(ResultsGameActivity));
            intent.PutExtra("isEnd", false);
            this.StartActivityWithoutBackStack(intent);
        }
        private void EnumeratorOnCurrentPositionChanged(object sender, PlayerPositionChangedEventArgs playerPositionChangedEventArgs)
        {
            this.SaveCurrentPlayer(_playerEnumerator.CurrentPosition);
        }

        private async void PlayersOnScoreChanged(object sender, ScoreChangedEventArgs scoreChangedEventArgs)
        {
            await Rep.Instance.SavePlayersAsync();
        }


        #endregion
        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);
                SetContentView(Resource.Layout.StartGame);
                SetHomeButtonEnabled(false);

                Initialize();
                InitializeTaskProgressBox();
                RegisterSubscribes();
                RefreshPoints();
                RefreshPlayer();
                SetButtonsEnabled(false);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "OnCreate", exception, true);
            }
        }



        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                this.SaveAsStartupActivity(StringConst.GameActivity);
                if (Rep.DatabaseHelper.Tasks.Count == Rep.DatabaseHelper.Tasks.Enumerator.CurrentPosition + 1)
                    GetShopDialog().Show();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "OnResume", exception, true);
            }

        }
        private void CardImageOnClick(object sender, EventArgs eventArgs)
        {
            var intent = new Intent(this, typeof(ImageActivity));
            StartActivityForResult(intent, 100);
            SetButtonsEnabled(true);
        }

        private void RefuseButtonOnClick(object sender, EventArgs eventArgs)
        {
            _taskDialog = _taskDialog == TaskDialog.RefuseFirstTime ? TaskDialog.RefuseSecondTime : TaskDialog.RefuseFirstTime;
            var score = _taskDialog == TaskDialog.RefuseFirstTime
                ? _taskEnumerator.Current.Score * 2
                : _taskEnumerator.Current.Score;
            ShowDialogTask(_playerEnumerator.Current, score);
        }

        private void MakeThisButtonOnClick(object sender, EventArgs eventArgs)
        {
            var score = _taskDialog == TaskDialog.RefuseFirstTime
                ? _taskEnumerator.Current.Score * 2
                : _taskEnumerator.Current.Score;
            _taskDialog = TaskDialog.Make;
            _playerEnumerator.Current.Score += score;
            ShowDialogTask(_playerEnumerator.Current, score);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                if (resultCode == Result.Canceled) return;
                if (requestCode == 100)
                {
                    StartActivityForResult(typeof(ChoosePlayerActivity), 101);
                }
                if (requestCode == 101)
                {
                    var position = data.GetIntExtra("player_id", -1);
                    if (position < 0)
                        throw new InvalidOperationException("You should choose player");
                    var player = Rep.Instance.Players[position];
                    player.Score -= 5;
                    _taskDialog = TaskDialog.Bear;
                    ShowDialogTask(player, _taskEnumerator.Current.Score);
                }
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "OnActivityResult", exception, false);
            }
        }
        private void ConinueButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (TaskDialog.Make == _taskDialog)
            {
                StartActivity(typeof(BonusActivity));
            }
            ShowTaskProgress(false);

            if (_taskDialog != TaskDialog.RefuseFirstTime && !MoveNextTask())
                GetShopDialog().Show();
            else
                MoveNextStep(_taskDialog == TaskDialog.RefuseFirstTime);
        }

        private void MoveNextStep(bool pointsIsDoubled)
        {
            RefreshPoints(pointsIsDoubled);
            MoveNextPlayer();
            SetButtonsEnabled(false);
        }


        private void SetButtonsEnabled(bool enabled)
        {
            try
            {
                _refuseButton.Enabled = enabled;
                _refuseButton.Background.SetAlpha(enabled ? 255 : 50);
                _refuseButton.SetTextColor(Resources.GetColor(enabled
                    ? Resource.Color.white
                    : Resource.Color.pressed_text_color));

                _makeThisButton.Enabled = enabled;
                _makeThisButton.Background.SetAlpha(enabled ? 255 : 50);
                _makeThisButton.SetTextColor(Resources.GetColor(enabled
                    ? Resource.Color.white
                    : Resource.Color.pressed_text_color));
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "SetButtonsEnabled", exception, false);
            }
        }

        private void ShowDialogTask(Player player, int taskScore)
        {
            try
            {
                var taskStrings = TaskDialogBuilder.GetTask(_taskDialog, taskScore, player.GetModel());

                var color = _taskDialog == TaskDialog.Make ? _greenColor : _redColor;
                _statusTitleText.SetTextColor(color);
                _statusDescriptionText.SetTextColor(color);

                _statusTitleText.Text = taskStrings.GetStatusTitle();
                _statusDescriptionText.Text = taskStrings.GetStartDesc();
                _changeCountScoreText.Text = taskStrings.GetChangedScore();
                _endDescriptionText.Text = taskStrings.GetEndDesc();
                _currentScoreText.Text = taskStrings.GetCurentScore();

                ShowTaskProgress(true);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "ShowDialogTask", exception, true);
            }

        }

        void ShowTaskProgress(bool isShow)
        {
            try
            {
                _taskProgressView.Visibility = isShow ? ViewStates.Visible : ViewStates.Gone;
                _cardImage.Enabled = !isShow;
                _actionButton.Enabled = !isShow;
                _refuseButton.Enabled = !isShow;
                _makeThisButton.Enabled = !isShow;
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "ShowTaskProgress", exception, false);
            }

        }

        void MoveNextPlayer()
        {
            try
            {
                _playerEnumerator.MoveNext();
                RefreshPlayer();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "MoveNextPlayer", exception, false);
            }

        }

        void RefreshPoints(bool isDoubled = false)
        {
            try
            {
                _x2.Visibility = isDoubled ? ViewStates.Visible : ViewStates.Invisible;
                _points.Text = (_taskEnumerator.Current.Score == 0
                        ? new Random().Next(3) + 1
                        : _taskEnumerator.Current.Score)
                    .ToString();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "RefreshPoints", exception, false);
            }

        }
        void RefreshPlayer()
        {
            try
            {
                var player = _playerEnumerator.Current;
                FindViewById<ImageView>(Resource.Id.playerInGameImage).SetImageBitmap(player.Image);
                _playerName.Text = player.Name;
                _playerPosition.Text = Resources.GetString(Resource.String.PositionPlayerString) + Rep.Instance.Players.GetPosition(player);
                _playerScore.Text = player.Score.ToString();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "RefreshPlayer", exception, false);
            }

        }

        private bool MoveNextTask()
        {
            return _taskEnumerator.MoveNext();
        }

        public override void OnBackPressed()
        {
            try
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetMessage(GetString(Resource.String.AreYouShureString));
                builder.SetCancelable(true);
                builder.SetPositiveButton(GetString(Resource.String.YesString), async (sender, args) => base.OnBackPressed ());
                builder.SetNegativeButton(GetString(Resource.String.NoString), (sender, args) =>{});
                builder.Create().Show();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "OnBackPressed", exception, false);
            }

        }
    }
}