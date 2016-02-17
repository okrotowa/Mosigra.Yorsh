using System;
using Yorsh.Data;
using Yorsh.Helpers;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Content;
using Android.Graphics;
using Yorsh.Adapters;

namespace Yorsh.Activities
{
    [Activity(Label = "@string/PlayersString", ParentActivity = typeof(MainMenuActivity),ScreenOrientation = ScreenOrientation.Portrait)]
    public class AddPlayersActivity : BaseActivity
    {
        private Button _startGameButton;
        private bool _isPlayersCountValidate;
        private ListView _listView;

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
                _listView = FindViewById<ListView>(Resource.Id.playersList);
                Rep.Instance.Players.ItemRemoved += PlayersOnItemRemoved;
				_startGameButton.Click += _startGameButton_Click;
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "OnCreate", exception, false);
            }

        }

        private void PlayersOnItemRemoved(object sender, EventArgs eventArgs)
        {
            IsPlayersCountValidate = Rep.Instance.Players.Count > 1;
            _listView.Adapter = new AddNewPlayerListAdapter(this);
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                _listView.Adapter = new AddNewPlayerListAdapter(this);
                IsPlayersCountValidate = Rep.Instance.Players.Count > 1;

            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "OnResume", exception, false);
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
                    GaService.TrackAppException(this.Class, "IsPlayersCountValidate", exception, false);
                }
            }
        }

		async void  _startGameButton_Click(object sender, EventArgs e)
		{
			if (IsPlayersCountValidate) 
			{
				await Rep.Instance.ClearAsync();
				this.SaveCurrentPlayer(0);
				this.StartActivityWithoutBackStack (new Intent (this, typeof(GameActivity)));			
			} 
			else 
			{
				this.StartActivityWithoutBackStack (new Intent (this, typeof(AddNewPlayerActivity)));
			}
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
                GaService.TrackAppException(this.Class, "SetButtonEnabled", exception, false);
            }
        }


        protected override void OnPause()
        {
            try
            {
                _startGameButton.Background.ClearColorFilter();
                base.OnPause();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "OnPause", exception, false);
            }
        }
    }
}