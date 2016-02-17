using System;
using System.Reflection;
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
        private readonly ImageActivityHelper _imageActivityHelper = Rep.DatabaseHelper.ImageActivityHelper;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetContentView(Resource.Layout.ImageCard);
                //_taskId = Intent.Extras.GetInt("taskId");
                //task = Rep.DatabaseHelper.Tasks.GetTask(_taskId);
                //var category = Rep.DatabaseHelper.Tasks.GetCategory(task.CategoryId);
                var image = FindViewById<ImageView>(Resource.Id.imageCardView);
                image.SetImageBitmap(_imageActivityHelper.CategoryImage);

                var text = FindViewById<TextView>(Resource.Id.textCard);
                text.SetTypeface(this.BankirRetroFont(), TypefaceStyle.Normal);
                text.Text = _imageActivityHelper.Task.TaskName;

                _contentFrameLayout = FindViewById(Resource.Id.contentFrameLayout);

                //using (var resourceStream = ResourceLoader.GetEmbeddedResourceStream(Assembly.GetAssembly(typeof(ResourceLoader)), category.ImageName))
                //{
                //    image.SetImageBitmap(BitmapFactory.DecodeStream(resourceStream));
                //}

                //text.Text = task.TaskName;
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