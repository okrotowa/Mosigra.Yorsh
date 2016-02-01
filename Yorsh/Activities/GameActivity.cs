using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using SQLite;
using Yorsh.Data;
using Yorsh.Helpers;
using Yorsh.Model;

namespace Yorsh.Activities
{
    [Activity(Label = "@string/GameString", ParentActivity = typeof(MainMenuActivity), ScreenOrientation = ScreenOrientation.Portrait)]
    public class GameActivity : BaseActivity
    {
        bool _isRefusing;
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
        TaskDialog _taskDialog;
        bool _shouldSaveContext;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);
                SetContentView(Resource.Layout.StartGame);
                SetHomeButtonEnabled(false);
                Initialize();
                _taskEnumerator = Rep.DatabaseHelper.Tasks.Enumerator;

                _cardImage.Click += (sender, args) =>
                {
                    _shouldSaveContext = false;
                    var intent = new Intent(this, typeof(ImageActivity));
                    intent.PutExtra("taskId", _taskEnumerator.Current.Id);
                    StartActivityForResult(intent, 100);
                    SetButtonsEnabled(true);
                };

                _makeThisButton.Touch += (sender, args) => this.OnTouchButtonDarker(_makeThisButton, args);
                _makeThisButton.Click += delegate
                {
                    Rep.Instance.Players.Enumerator.Current.Score += _taskEnumerator.Current.Score;
                    ShowDialogOnButtonClick(TaskDialog.Make);
                };

                _refuseButton.Touch += (sender, args) => this.OnTouchButtonDarker(_refuseButton, args);
                _refuseButton.Click += delegate
                {
                    if (!_isRefusing) _taskEnumerator.Current.Score *= 2;
                    ShowDialogOnButtonClick(_isRefusing ? TaskDialog.RefuseAndMove : TaskDialog.Refuse);
                };

                _actionButton.Click += delegate
                {
                    var intent = new Intent(this, typeof(ResultsGameActivity));
                    intent.PutExtra("isEnd", false);
                    this.StartActivityWithoutBackStack(intent);
                };
                InitPlayers();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "OnCreate", exception, true);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                _shouldSaveContext = true;
                if (Rep.DatabaseHelper.Tasks.Count == _taskEnumerator.CurrentPosition + 1)
                    GetShopDialog().Show();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "OnResume", exception, true);
            }

        }

        private void Initialize()
        {
            try
            {
                _playerName = FindViewById<TextView>(Resource.Id.playerInGameName);
                _playerPosition = FindViewById<TextView>(Resource.Id.playerInGamePostion);
                _playerScore = FindViewById<TextView>(Resource.Id.playerInGameScore);
                _makeThisButton = FindViewById<Button>(Resource.Id.makeThisButton);
                _refuseButton = FindViewById<Button>(Resource.Id.refuseButton);
                _cardImage = FindViewById<ImageButton>(Resource.Id.taskImageButton);
                _actionButton = (TextView)CreateActionButton(Resource.Drawable.table_button);
                _points = FindViewById<TextView>(Resource.Id.points);
                _x2 = FindViewById<TextView>(Resource.Id.x2);

                _playerName.SetTypeface(this.MyriadProFont(MyriadPro.Bold), TypefaceStyle.Normal);
                _playerPosition.SetTypeface(this.MyriadProFont(MyriadPro.Regular), TypefaceStyle.Normal);
                _makeThisButton.SetTypeface(this.MyriadProFont(MyriadPro.BoldCondensed), TypefaceStyle.Normal);
                _refuseButton.SetTypeface(this.MyriadProFont(MyriadPro.BoldCondensed), TypefaceStyle.Normal);
                _playerScore.SetTypeface(this.MyriadProFont(MyriadPro.BoldCondensed), TypefaceStyle.Normal);
                _points.SetTypeface(this.MyriadProFont(MyriadPro.Bold), TypefaceStyle.Normal);
                _x2.SetTypeface(this.MyriadProFont(MyriadPro.Bold), TypefaceStyle.Normal);
                _cardImage.SetImageResource(Resource.Drawable.card_backside);


                FindViewById<ImageButton>(Resource.Id.continueButton).Click += (sender, args) =>
                {
                    if (TaskDialog.Make == _taskDialog)
                    {
                        StartActivity(typeof(BonusActivity));
                    }
                    ShowTaskProgress(false);
                };
                FindViewById<TextView>(Resource.Id.scoreString).SetTypeface(this.MyriadProFont(MyriadPro.Condensed), TypefaceStyle.Normal);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "Initialize", exception, true);
            }
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
                GaService.TrackAppException(this.Class.SimpleName, "SetButtonsEnabled", exception, true);
            }
        }


        private void ShowDialogOnButtonClick(TaskDialog taskDialog)
        {
            ShowDialogOnButtonClick(taskDialog, Rep.Instance.Players.Enumerator.Current);
        }

        public void ShowDialogOnButtonClick(TaskDialog taskDialog, Player currentPlayer)
        {

            ShowDialogTask(taskDialog, currentPlayer);
        }

        private void ShowDialogTask(TaskDialog taskDialog, Player currentPlayer)
        {
            try
            {
                _taskDialog = taskDialog;
                var taskScore = Rep.DatabaseHelper.Tasks.Current.Score;
                var taskStrings = TaskDialogBuilder.GetTask(_taskDialog, taskScore, currentPlayer.GetModel());
                var color = Resources.GetColor(_taskDialog == TaskDialog.Make
                    ? Resource.Color.task_progress_green
                    : Resource.Color.task_progress_red);

                var statusTitleText = FindViewById<TextView>(Resource.Id.statusTitleText);
                statusTitleText.SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), TypefaceStyle.Normal);
                statusTitleText.SetTextColor(color);
                statusTitleText.Text = taskStrings.GetStatusTitle();

                var statusDescriptionText = FindViewById<TextView>(Resource.Id.statusDescriptionText);
                statusDescriptionText.SetTypeface(this.MyriadProFont(MyriadPro.Condensed), TypefaceStyle.Normal);
                statusDescriptionText.SetTextColor(color);
                statusDescriptionText.Text = taskStrings.GetStartDesc();

                var boldConsededFont = this.MyriadProFont(MyriadPro.BoldCondensed);
                var changeCountScoreText = FindViewById<TextView>(Resource.Id.changeCountScoreText);
                changeCountScoreText.SetTypeface(boldConsededFont, TypefaceStyle.Normal);
                changeCountScoreText.Text = taskStrings.GetChangedScore();

                var endDescriptionText = FindViewById<TextView>(Resource.Id.endDescriptionText);
                endDescriptionText.SetTypeface(this.MyriadProFont(MyriadPro.LightCondensed), TypefaceStyle.Normal);
                endDescriptionText.Text = taskStrings.GetEndDesc();

                var currentScoreText = FindViewById<TextView>(Resource.Id.currentScoreText);
                currentScoreText.SetTypeface(boldConsededFont, TypefaceStyle.Normal);
                currentScoreText.Text = taskStrings.GetCurentScore();

                ShowTaskProgress(true);

                _isRefusing = _taskDialog == TaskDialog.Refuse;
                if (!_isRefusing && !MoveNextTask())
                    EndOfGame();
                else
                    MoveNextStep();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "ShowDialogTask", exception, true);
            }
           
        }

        void ShowTaskProgress(bool isShow)
        {
            try
            {
                var taskProgressView = FindViewById<RelativeLayout>(Resource.Id.taskProgressView);
                taskProgressView.Visibility = isShow ? ViewStates.Visible : ViewStates.Gone;
                if (isShow)
                {
                    FindViewById<RelativeLayout>(Resource.Id.startGameLayout)
                        .Background.SetColorFilter(this.Resources.GetColor(Resource.Color.half_black), PorterDuff.Mode.SrcAtop);
                    _cardImage.Enabled = false;
                    this.ActionButton.Enabled = false;
                }
                else
                {
                    FindViewById<RelativeLayout>(Resource.Id.startGameLayout).Background.ClearColorFilter();
                    _cardImage.Enabled = true;
                    this.ActionButton.Enabled = true;
                }
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "ShowTaskProgress", exception, false);
            }
           
        }

        void MoveNextStep()
        {
            try
            {
                Rep.Instance.Players.Enumerator.MoveNext();
                InitPlayers();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "MoveNextStep", exception, false);
            }

        }

        void InitPlayers()
        {
            try
            {
                _x2.Visibility = _isRefusing ? ViewStates.Visible : ViewStates.Invisible;
                _points.Text = (_isRefusing ? _taskEnumerator.Current.Score / 2
                    : _taskEnumerator.Current.Score == 0
                        ? new Random().Next(3) + 1
                        : _taskEnumerator.Current.Score)
                    .ToString();
                var player = Rep.Instance.Players.Enumerator.Current;
                FindViewById<ImageView>(Resource.Id.playerInGameImage).SetImageBitmap(player.Image);
                _playerName.Text = player.Name;
                _playerPosition.Text = Resources.GetString(Resource.String.PositionPlayerString) + Rep.Instance.Players.GetPosition(player);
                _playerScore.Text = player.Score.ToString();
                SetButtonsEnabled(false);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "InitPlayers", exception, false);
            }
            
        }

        void EndOfGame()
        {
            try
            {
                GetShopDialog().Show();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "EndOfGame", exception, false);
            }
        }

        private AlertDialog.Builder GetShopDialog()
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

        public override void OnBackPressed()
        {
            try
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetMessage(GetString(Resource.String.AreYouShureString));
                builder.SetCancelable(true);
                AllowBackPressed = false;
                builder.SetPositiveButton(GetString(Resource.String.YesString), async (sender, args) =>
                {
                    await Rep.Instance.ClearAsync();
                    this.SaveCurrentPlayer(0);
                    base.OnBackPressed();
                });
                builder.SetNegativeButton(GetString(Resource.String.NoString), (sender, args) =>
                {
                });

                var alertDialog = builder.Create();
                alertDialog.Show();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "OnBackPressed", exception, false);
            }
           
        }



        /// <exception cref="T:System.InvalidOperationException"></exception>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                if (resultCode == Result.Canceled) return;
                if (requestCode == 100)
                {

                    _shouldSaveContext = false;
                    StartActivityForResult(typeof(ChoosePlayerActivity), 101);
                }
                if (requestCode == 101)
                {
                    _shouldSaveContext = true;
                    var position = data.GetIntExtra("player_id", -1);
                    if (position < 0)
                        throw new InvalidOperationException("You should choose player");
                    var player = Rep.Instance.Players[position];
                    player.Score -= 5;
                    ShowDialogOnButtonClick(TaskDialog.Bear, player);
                }
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "OnActivityResult", exception, false);
            }
           

        }

        private bool MoveNextTask()
        {
            return _taskEnumerator.MoveNext();
        }

        protected override void OnPause()
        {
            try
            {
                this.SaveAsStartupActivity(StringConst.GameActivity);
                if (_shouldSaveContext)
                {
                    Rep.Instance.SavePlayers();
                    this.SaveCurrentPlayer(Rep.Instance.Players.Enumerator.CurrentPosition);
                    Task.Run(() => Rep.DatabaseHelper.SaveTaskContextAsync());
                }
                base.OnPause();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "OnPause", exception, false);
            }
           
        }

        protected override void OnDestroy()
        {
            SetButtonsEnabled(true);
            base.OnDestroy();
        }
    }
}