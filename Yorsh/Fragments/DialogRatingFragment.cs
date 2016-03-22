using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Yorsh.Fragments
{		
	public sealed class DialogRatingFragment : DialogFragment
	{
	    private ISharedPreferencesEditor _editor;
		private bool _editorIsPutted = false;

	    public event EventHandler Dissmissed;

	    private void OnDissmissed()
	    {
	        var handler = Dissmissed;
	        if (handler != null) handler(this, EventArgs.Empty);
	    }

	    public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{
			var dialog = base.OnCreateDialog(savedInstanceState);
			dialog.Window.RequestFeature(WindowFeatures.NoTitle);
			dialog.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen); 
			dialog.Window.SetBackgroundDrawableResource(Resource.Color.white);
			return dialog;
		}

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.DialogRating, null);
			var buttonEasy = view.FindViewById<Button>(Resource.Id.buttonEasy);
			var buttonYester = view.FindViewById<Button>(Resource.Id.buttonYester);
			var buttonNo = view.FindViewById<Button>(Resource.Id.buttonNo);

            var preferences = Activity.GetPreferences(FileCreationMode.Private);
            _editor = preferences.Edit();   
     
			buttonEasy.Click += (object sender, EventArgs e) => 
            {
                var uri = Android.Net.Uri.Parse("market://details?id=" + Activity.PackageName);
                var goToMarket = new Intent(Intent.ActionView, uri);
                try
                {
                    StartActivity(goToMarket);
                }
                catch (ActivityNotFoundException)
                {
                    StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"http://play.google.com/store/apps/details?id=" + Activity.PackageName)));
                }
                PutEditor(false);
                this.Dismiss();
			};

			buttonYester.Click += (object sender, EventArgs e) => {
				PutEditor(true);
                this.Dismiss();
			};

			buttonNo.Click += (object sender, EventArgs e) => {
				PutEditor(false);
                this.Dismiss();
			};
            return view;
        }
		private void PutEditor(bool value)
		{
			_editorIsPutted = true;
			_editor.PutBoolean("isShow", value);
		}


	    public override void Dismiss()
        {
			if (!_editorIsPutted) PutEditor (true);
			_editor.Commit();
            OnDissmissed();
            base.Dismiss();
        }
	}
}

