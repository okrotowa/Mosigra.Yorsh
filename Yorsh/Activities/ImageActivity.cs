using System;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using Yorsh.Data;
using Yorsh.Helpers;
using Android.Content.PM;

namespace Yorsh.Activities
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", ParentActivity = typeof(GameActivity), ScreenOrientation = ScreenOrientation.Portrait)]
    public class ImageActivity : Activity
    {
        private View _contentFrameLayout;
        private readonly ImageActivityHelper _imageActivityHelper = Rep.DatabaseHelper.ImageActivityHelper;
        private ImageView _image;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetContentView(Resource.Layout.ImageCard);
                _image = FindViewById<ImageView>(Resource.Id.imageCardView);
                _image.SetImageBitmap(_imageActivityHelper.CategoryImage);

                var text = FindViewById<TextView>(Resource.Id.textCard);
                text.SetTypeface(Rep.FontManager.Get(Font.BankirRetro), TypefaceStyle.Normal);
                text.Text = _imageActivityHelper.Task.TaskName;

                _contentFrameLayout = FindViewById(Resource.Id.contentFrameLayout);

                _contentFrameLayout.Click += ContentFrameLayoutOnClick;

            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class,"OnCreate", exception, false);
                base.OnBackPressed();
            }
        }

        private void ContentFrameLayoutOnClick(object sender, EventArgs eventArgs)
        {
            SetResult();
        }

        private void SetResult()
        {
            try
            {
                var isBear = _imageActivityHelper.Task.IsBear;
                SetResult(isBear ? Result.Ok : Result.Canceled);
                _contentFrameLayout.Click -= ContentFrameLayoutOnClick;
				if (_image!=null && _image.Drawable!=null) _image.Drawable.Dispose();
                base.OnBackPressed();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "SetResult", exception, false);
                throw;
            }
            
        }
        public override void OnBackPressed()
        {
           SetResult();
        }
    }
}