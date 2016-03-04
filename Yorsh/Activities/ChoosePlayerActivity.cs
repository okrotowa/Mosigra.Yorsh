using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;
using Yorsh.Adapters;

namespace Yorsh.Activities
{
	[Activity (Label = "@string/PickPlayerString", ParentActivity = typeof(GameActivity))]			
	public class ChoosePlayerActivity : BaseActivity
	{
		TextView _readyButton;
		PlayerListAdapter _adapter;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.ChoosePlayer);
			SetHomeButtonEnabled (false);
			var list = this.FindViewById<ListView>(Resource.Id.playerList);
			_adapter = new PlayerListAdapter(this);

			list.Adapter = _adapter;
			_readyButton = (TextView)CreateActionButton(Resource.Drawable.table_button);
			SetTextButtonEnabled (false);
			_readyButton.SetText (Resource.String.ConfirmString);
			_readyButton.SetTextColor (Resources.GetColorStateList(Resource.Drawable.ready_label_enable));
			
            RegisterSubscribes();
		}

	    private void ReadyButtonOnClick(object sender, EventArgs eventArgs)
	    {
            SetResult(Result.Ok, new Intent().PutExtra("player_id", _adapter.SelectedPosition));
            this.Finish();
	    }

	    protected override void RegisterSubscribes()
	    {
            _readyButton.Click += ReadyButtonOnClick; 
	    }

	    protected override void UnregisterSubscribes()
	    {
            _readyButton.Click -= ReadyButtonOnClick; 
	    }

	    //After ReadyButton Initialization
		public void SetTextButtonEnabled(bool enabled)
		{
			_readyButton.Enabled = enabled;
		}


		public override void OnPreBackPressed ()
		{
			AllowBackPressed = false;
			var toast = Toast.MakeText (this, _adapter.SelectedView == null 
					? Resource.String.PleaseSelectThePlayerString
					: Resource.String.PressReadyButtonString, 
				ToastLength.Short);
			toast.Show ();
		}
	}
}

