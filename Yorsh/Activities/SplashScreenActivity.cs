using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Java.Lang;
using Yorsh.Helpers;
using Yorsh.Model;
using Exception = System.Exception;

namespace Yorsh.Activities
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true, NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreenActivity : Activity
    {
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Splash);

            Intent intent;
            try
            {
                var prefs = GetSharedPreferences("X", FileCreationMode.Private);
                var className = prefs.GetString("lastActivity", null);
                if (className == null)
                {
                    intent = new Intent(this,typeof(MainMenuActivity));
                }
                else
                {
                    intent = new Intent(this,Class.ForName(className));
                    if (className.Contains("Results")) intent.PutExtra("isEnd", true);
                }
            }
            catch (ClassNotFoundException)
            {
                intent = new Intent(this, typeof (MainMenuActivity));
            }

            var prefsT = GetSharedPreferences("T", FileCreationMode.Private);
            
            await this.CreateDataBaseAsync();
            await Rep.Instance.InitializeRepositoryAsync();
            Rep.Instance.Tasks.SetCurrent(prefsT.GetInt("currentTask", -1));
            Rep.Instance.Players.SetCurrent(prefsT.GetInt("currentPlayer", -1));
            StartActivity(intent);
        }
    }
}