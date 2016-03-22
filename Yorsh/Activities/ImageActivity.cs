using System;
using Android.App;
using Android.Graphics;
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
        private ImageView _image;
        private TaskTable _task;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                _task = Rep.DatabaseHelper.Tasks.Enumerator.Current;
                var category = Rep.DatabaseHelper.Tasks.GetCategory(_task.CategoryId);
                SetContentView(Resource.Layout.ImageCard);
                _image = FindViewById<ImageView>(Resource.Id.imageCardView);
                var imageIdentifier = this.Resources.GetIdentifier(category.Image, "drawable", this.PackageName);
                _image.SetImageResource(imageIdentifier);

                var text = FindViewById<TextView>(Resource.Id.textCard);
                text.SetTypeface(Rep.FontManager.Get(Font.BankirRetro), TypefaceStyle.Normal);
                text.Text = _task.TaskName;

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
                var isBear = _task.IsBear;
                SetResult(isBear ? Result.Ok : Result.Canceled);
                _contentFrameLayout.Click -= ContentFrameLayoutOnClick;

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

        protected override void OnDestroy()
        {
            if (_image != null && _image.Drawable != null)
            {
                _image.Drawable.Dispose();
                _image.SetImageBitmap(null);
            }
            base.OnDestroy();
        }
    }
}