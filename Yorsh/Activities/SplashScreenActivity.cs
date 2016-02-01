using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Analytics;
using Android.OS;
using Xamarin.InAppBilling;
using Yorsh.Data;
using Yorsh.Helpers;
using Yorsh.Model;
using Android.Widget;

namespace Yorsh.Activities
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true, NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreenActivity : Activity
    {
        InAppBillingServiceConnection _serviceConnection;
        private bool _startActivity;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Splash);
            InitGa();
            InitConnectToGoogleStore();
        }

        protected override void OnResume()
        {
            base.OnResume();
            _serviceConnection.Connect();
            CreateLocalDatabase();
        }

        private async void DatabaseHelperOnDatabaseCreatedOrOpened(object sender, EventArgs eventArgs)
        {
            await Task.Delay(1000);
            if (!_serviceConnection.Connected || _serviceConnection.BillingHandler == null)
            {
                Toast.MakeText(this, "Нет подключения к Google Store", ToastLength.Short);
            }
            else
            {
                await RefreshPurchaseItems();
            }

            await StartActivityAsync();
        }


        private void InitConnectToGoogleStore()
        {
            try
            {
                var key = Xamarin.InAppBilling.Utilities.Security.Unify(new[]
            {
                GetNumberString(3),
                GetNumberString(6),
                GetNumberString(1),
                GetNumberString(4),
                GetNumberString(2),
                GetNumberString(7),
                GetNumberString(0),
                GetNumberString(5)
            },
                new[] { 0, 1, 2, 3, 4, 5, 6, 7 }
                );
                _serviceConnection = new InAppBillingServiceConnection(this, key);
                _serviceConnection.BindErrors();
                _serviceConnection.OnConnected += () =>
                {
                    if (_serviceConnection.BillingHandler!=null) _serviceConnection.BillingHandler.BindErrors();
                }; 
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.Name, "InitConnectToGoogleStore", exception, false);
            }

        }

        private async Task StartActivityAsync()
        {
            try
            {
                if (_startActivity) return;
                _startActivity = true;
                var playerIsGenerated = await Rep.Instance.InitPlayersAsync();
                var intent = GetIntentStartActivity(!playerIsGenerated);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "StartActivityAsync",exception, true);
            }

        }

        private Intent GetIntentStartActivity(bool isDefault)
        {
            if (isDefault)
            {
                Rep.DatabaseHelper.Tasks.SetCurrent(0);
                Rep.Instance.Players.Enumerator.SetCurrent(0);
                return new Intent(this, typeof(MainMenuActivity));
            }

            var prefX = GetSharedPreferences("X", FileCreationMode.Private);
            var className = prefX.GetString("lastActivity", null);

            switch (className)
            {
                case StringConst.GameActivity:
                    return new Intent(this, typeof(GameActivity));
                case StringConst.MainMenuActivity:
                    return new Intent(this, typeof(MainMenuActivity));
                case StringConst.ResultGameActivity:
                    var intent = new Intent(this, typeof(ResultsGameActivity));
                    intent.PutExtra("isEnd", true);
                    return intent;
                default:
                    return new Intent(this, typeof(MainMenuActivity));
            }
        }
        private void CreateLocalDatabase()
        {
            try
            {
                if (System.IO.File.Exists(Rep.Instance.OldDataBaseFile)) System.IO.File.Delete(Rep.Instance.OldDataBaseFile);
                Rep.Instance.InitDataBase(Application);
                Rep.DatabaseHelper.DataBaseCreatedOrOpened += DatabaseHelperOnDatabaseCreatedOrOpened;
                Task.Factory.StartNew(() => Rep.DatabaseHelper.CreateOrOpenDataBaseAsync());
            }
            catch (Exception ex)
            {
                GaService.TrackAppException(this.Class.SimpleName, "CreateLocalDatabase", ex, false);
            }

        }
        private string GetNumberString(int num)
        {
            var s = new[]
			{
				@"bRI7RgmYxy8/Y9Uy4I3njTVvbpocAaxrckdwDT5Dq7L/aWzHN/WIcJpf",
				@"xtcU5stKVVlGG6im5KHJo6ZRIp2foVhJqo8x3EVibcevuS/4pPOc",
				@"NDRv+5jb4NOXPvJsaZtgk4sxjJ9UnJWaJe1wdiAAfpnl5GYoTLkV",   
				@"MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAwa50OL",
				@"u/CZWS2NRvBS+Lv/dESNl8XOKZ5qkCijHKIqQSyRXo//9KvEY",
				@"jEBrKIMHi3ANIHp53hgHQIDAQAB",
				@"RNqP9KW4wEeS70FUhQkWgV46HtdGX5bJMpdGKxPIq8h4MFhs87O",
				@"cVhs7LDVLl/Ip3ozOC6vKJPjJ/hn8ZdhBYRjfHnPzPgV5Bw9LhnBR9c",
			};

            return s[num];
        }

        async Task RefreshPurchaseItems()
        {
            try
            {
                var purchasese = _serviceConnection.Service.GetPurchases(3, this.PackageName,
                    ReservedTestProductIDs.Purchased, string.Empty);

                var purchased = _serviceConnection.BillingHandler.GetPurchases(ItemType.Product);
                var ershPurchases = purchased == null
                    ? new List<ErshPurchase>()
                    : purchased.Where(purchase => purchase.PurchaseState == 0).Select(purchase => new ErshPurchase(purchase.ProductId)).Where(purchase => purchase.IsImplemented).ToList();
                await Rep.DatabaseHelper.CheckAndRefreshPurshases(ershPurchases, _serviceConnection.Connected);
            }
            catch (Exception ex)
            {
                GaService.TrackAppException(this.Class.SimpleName, "RefreshPurchaseItems", ex, false);
            }

        }


        private void InitGa()
        {
            Rep.Instance.GaInstance = GoogleAnalytics.GetInstance(ApplicationContext);
            Rep.Instance.GaInstance.SetLocalDispatchPeriod(10);

            Rep.Instance.GaTracker = Rep.Instance.GaInstance.NewTracker("UA-71751285-2");
            Rep.Instance.GaTracker.EnableExceptionReporting(true);
            Rep.Instance.GaTracker.EnableAdvertisingIdCollection(true);
            Rep.Instance.GaTracker.EnableAutoActivityTracking(true);
        }


        protected override void OnDestroy()
        {
            _serviceConnection.UnbindErrors();
            Rep.DatabaseHelper.DataBaseCreatedOrOpened -= DatabaseHelperOnDatabaseCreatedOrOpened;
            if (_serviceConnection.Connected)
            {
                _serviceConnection.Disconnect();
                if (_serviceConnection.BillingHandler != null) _serviceConnection.BillingHandler.UnbindErrors();
            }
            base.OnDestroy();
        }
    }
}