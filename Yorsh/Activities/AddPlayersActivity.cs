using System;
using Yorsh.Helpers;
using Yorsh.Model;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Content;
using Android.Graphics;
using Yorsh.Adapters;

namespace Yorsh.Activities
{
    [Activity(Label = "@string/PlayersString", ParentActivity = typeof(MainMenuActivity),
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class AddPlayersActivity : BaseActivity
    {
        private Button _startGameButton;
        private bool _isPlayersCountValidate;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);
                SetContentView(Resource.Layout.AddPlayers);
                CreateActionButton(Resource.Drawable.add_player_button).Click += (sender, e) =>
                    this.StartActivityWithoutBackStack(new Intent(this, typeof(AddNewPlayerActivity)));
                _startGameButton = FindViewById<Button>(Resource.Id.startPlayButton);
                _startGameButton.Touch += (sender, e) => this.OnTouchButtonDarker(_startGameButton, e);
                _startGameButton.SetTypeface(this.MyriadProFont(MyriadPro.BoldCondensed), TypefaceStyle.Normal);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "OnCreate", exception, false);
            }

        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                var listView = FindViewById<ListView>(Resource.Id.playersList);
                listView.Adapter = new AddNewPlayerListAdapter(this);

                _startGameButton.Click += _startGameButton_Click;
                IsPlayersCountValidate = Rep.Instance.Players.Count > 1;
                Rep.Instance.Players.ItemRemoved += delegate
                {
                    {
                        IsPlayersCountValidate = Rep.Instance.Players.Count > 1;
                        listView.Adapter = new AddNewPlayerListAdapter(this);
                    }

                };
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "OnResume", exception, false);
            }
        }

        private bool IsPlayersCountValidate
        {
            get { return _isPlayersCountValidate; }
            set
            {
                try
                {
                    _isPlayersCountValidate = value;
                    int buttonNameString;
                    if (_isPlayersCountValidate)
                    {
                        buttonNameString = Resource.String.StartGameButtonString;
                        SetButtonEnabled(Rep.Instance.Players.IsAllPlay);
                    }
                    else
                    {
                        buttonNameString = Resource.String.AddNewPlayerString;
                        SetButtonEnabled(true);
                    }
                    _startGameButton.Text = Resources.GetString(buttonNameString);

                }
                catch (Exception exception)
                {
                    GaService.TrackAppException(this.Class.SimpleName, "IsPlayersCountValidate", exception, false);
                }
            }
        }

        void _startGameButton_Click(object sender, EventArgs e)
        {
            this.StartActivityWithoutBackStack(new Intent(this, !IsPlayersCountValidate ? typeof(AddNewPlayerActivity) : typeof(GameActivity)));
        }

        public void SetButtonEnabled(bool enabled)
        {
            try
            {
                if (_startGameButton.Enabled == enabled)
                    return;
                _startGameButton.Enabled = enabled;
                _startGameButton.SetTextColor(enabled ? Resources.GetColor(Resource.Color.white) : this.GetColorWithOpacity(Resource.Color.white, Resource.Color.button_text_disabled));
                if (enabled)
                    _startGameButton.Background.ClearColorFilter();
                else
                {
                    _startGameButton.Background.SetColorFilter(Resources.GetColor(Resource.Color.button_disabled), PorterDuff.Mode.SrcAtop);
                }
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "SetButtonEnabled", exception, false);
            }
        }


        protected override void OnPause()
        {
            try
            {
                _startGameButton.Background.ClearColorFilter();
                Rep.Instance.SavePlayers();
                base.OnPause();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "OnPause", exception, false);
            }
        }
    }
}