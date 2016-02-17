using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Yorsh.Fragments
{
    public sealed class ChoosePhotoDialog : DialogFragment
    {
		public event EventHandler MakePhoto;
        private void OnMakePhoto()
        {
            var handler = MakePhoto;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler ChoosePhoto;

        private void OnChoosePhoto()
        {
            EventHandler handler = ChoosePhoto;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        public override Dialog OnCreateDialog (Bundle savedInstanceState)
		{
            var dialog = base.OnCreateDialog(savedInstanceState);
            dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            dialog.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            dialog.Window.SetBackgroundDrawableResource(Resource.Color.white);
			return dialog;
		}
        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {			
            var view = inflater.Inflate(Resource.Layout.ChoosePhoto, container, false);
            view.FindViewById<Button>(Resource.Id.makePhotoButton).Click += delegate { OnMakePhoto(); this.Dismiss(); };
            view.FindViewById<Button>(Resource.Id.choosePhotoButton).Click += delegate { OnChoosePhoto(); this.Dismiss(); };
            view.FindViewById<Button>(Resource.Id.cancelButton).Click += delegate { Dismiss(); };
            return view;
        }
      
    }
}