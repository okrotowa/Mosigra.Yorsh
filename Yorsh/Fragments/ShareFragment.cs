using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Yorsh.Activities;

namespace Yorsh.Fragments
{
    public class ShareFragment : DialogFragment
    {
		public string token;
		public string userId;

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            var dialog = base.OnCreateDialog(savedInstanceState);
            dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            dialog.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            dialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.Transparent);
            return dialog;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
			var view = inflater.Inflate(Resource.Layout.ShareSoc, null);
			ImageButton btnFb = view.FindViewById<ImageButton>(Resource.Id.buttonFb);
			ImageButton btnVk = view.FindViewById<ImageButton>(Resource.Id.buttonVk);
			ImageButton btnTw = view.FindViewById<ImageButton>(Resource.Id.buttonTw);

			btnFb.Click += delegate {
				Activity.StartActivity(typeof(ShareToFacebookActivity));
				this.Dismiss();
			};

			btnVk.Click += (object sender, EventArgs e) => {
				Activity.StartActivity(typeof(ShareToVkActivity));
				this.Dismiss();
			};

			btnTw.Click += (object sender, EventArgs e) => {
				Activity.StartActivity(typeof(ShareToTwitterActivity));
				this.Dismiss();
			};
			return view;
        }

		public override void OnCancel(IDialogInterface dialog)
		{
			base.OnCancel(dialog);
		}

		public override void Dismiss()
		{
			base.Dismiss();
		}
			
	}
}