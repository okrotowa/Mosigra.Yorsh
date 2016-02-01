using System;
using System.Reflection;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Yorsh.Data;
using Yorsh.Helpers;
using Yorsh.Model;
using Android.Content.PM;

namespace Yorsh.Activities
{
	[Activity(Theme = "@android:style/Theme.NoTitleBar", ParentActivity = typeof(GameActivity), ScreenOrientation = ScreenOrientation.Portrait)]
    public class ImageActivity : Activity
    {
        private int _taskId;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {

                base.OnCreate(savedInstanceState);
                SetContentView(Resource.Layout.ImageCard);
                _taskId = Intent.Extras.GetInt("taskId");
                var task = Rep.DatabaseHelper.Tasks.GetTask(_taskId);
                var category = Rep.DatabaseHelper.Tasks.GetCategory(task.CategoryId);

                var image = FindViewById<ImageView>(Resource.Id.imageCardView);

                var text = FindViewById<TextView>(Resource.Id.textCard);

                text.SetTypeface(this.BankirRetroFont(), TypefaceStyle.Normal);
                //TODO: Problem with often click
                using (var resourceStream = ResourceLoader.GetEmbeddedResourceStream(
                    Assembly.GetAssembly(typeof (ResourceLoader)), category.ImageName))
                {
                    image.SetImageBitmap(BitmapFactory.DecodeStream(resourceStream));
                }
                text.Text = task.TaskName;
                FindViewById(Resource.Id.contentFrameLayout).Click += (sender, args) =>
                {
                    SetResult();
                    Finish();
                };
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "OnCreate", exception, false);
                Finish();
            }
        }
        
	    private void SetResult()
        {
            var isBear = Rep.DatabaseHelper.Tasks.GetTask(_taskId).IsBear;
            SetResult(isBear ? Result.Ok : Result.Canceled);
        }

    }
}