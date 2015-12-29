using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Yorsh.Helpers;
using Yorsh.Model;

namespace Yorsh.Activities
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true, NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreenActivity : Activity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Splash);
            Intent intent;
            var prefs = GetSharedPreferences("X", FileCreationMode.Private);
            var className = prefs.GetString("lastActivity", null);
            switch (className)
            {
                case ActivityExtensions.GameActivity:
                    intent = new Intent(this, typeof(GameActivity));
                    break;
                case ActivityExtensions.MainMenuActivity:
                    intent = new Intent(this, typeof(MainMenuActivity));
                    break;
                case ActivityExtensions.ResultGameActivity:
                    intent = new Intent(this, typeof(ResultsGameActivity));
                    intent.PutExtra("isEnd", true);
                    break;
                default: intent = new Intent(this, typeof(MainMenuActivity));
                    break;
            }

            var prefsT = GetSharedPreferences("T", FileCreationMode.Private);
            await this.CreateDataBaseAsync();
            await Rep.Instance.InitializeRepositoryAsync();
            var currentTask = prefsT.GetInt("currentTask", 0);
            Rep.Instance.Tasks.SetCurrent(currentTask);
            var currentPlayer = prefsT.GetInt("currentPlayer", 0);
            Rep.Instance.Players.SetCurrent(currentPlayer);
            StartActivity(intent);

        }
    }
}