using Android.App;
using Android.Content.PM;
using Android.OS;
using Yorsh.Helpers;

namespace Yorsh.Activities
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true, NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreenActivity : Activity
    {
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Splash);
            await this.CreateDataBaseAsync();
            StartActivity(typeof(MainMenuActivity));
        }
    }
}