using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Analytics;
using Android.OS;
using Android.Provider;
using LicenseVerificationLibrary;
using LicenseVerificationLibrary.Obfuscator;
using LicenseVerificationLibrary.Policy;
using Xamarin.InAppBilling;
using Yorsh.Data;
using Yorsh.Helpers;
using Yorsh.Model;
using Android.Widget;
using Uri = Android.Net.Uri;

namespace Yorsh.Activities
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true, NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreenActivity : Activity, ILicenseCheckerCallback
    {
        #region LicenseChecker

        private LicenseChecker _licenseChecker;
        private static readonly byte[] Salt =
        {
            11, 65, 30, 90, 103, 57, 74, 64, 51, 30, 95, 45, 23, 117, 36, 156, 11, 32, 55, 89
        };


        private void CheckLicense()
        {
            string deviceId = Settings.Secure.GetString(ContentResolver, Settings.Secure.AndroidId);

            // Construct the LicenseChecker with a policy.
            var obfuscator = new AesObfuscator(Salt, PackageName, deviceId);
            var policy = new ServerManagedPolicy(this, obfuscator);
            _licenseChecker = new LicenseChecker(this, policy, Xamarin.InAppBilling.Utilities.Security.Unify(new[] {
					GetNumberString (3),
					GetNumberString (6),
					GetNumberString (1),
					GetNumberString (4),
					GetNumberString (2),
					GetNumberString (7),
					GetNumberString (0),
					GetNumberString (5)
				}, new[] { 0, 1, 2, 3, 4, 5, 6, 7 }));
            _licenseChecker.CheckAccess(this);
        }

        public void Allow(PolicyServerResponse reason)
        {
            _pref1.Edit().PutString(_pref2, Base64.Encode(true.ToString())).Commit();
            OnLicenseChecked();
        }

        public void ApplicationError(CallbackErrorCode errorCode)
        {
            GaService.TrackAppEvent("ApplicationError", errorCode.ToString());
            OnLicenseChecked();
        }

        public void DontAllow(PolicyServerResponse reason)
        {
            switch (reason)
            {
                case PolicyServerResponse.NotLicensed:
                    _pref1.Edit().PutString(_pref2, Base64.Encode(false.ToString())).Commit();
                    GetDialog().Show();
                    break;
                default: OnLicenseChecked();
                    break;
            }
        }

        #endregion

        private InAppBillingServiceConnection _serviceConnection;
        private bool _startActivity;
        private ISharedPreferences _pref1;
        private string _pref2;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetContentView(Resource.Layout.Splash);
                InitGa();
                Rep.Instance.InitFontManager(Application);
                InitLicence();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(Class, "OnCreate", exception, true);
            }

        }

        private AlertDialog.Builder GetDialog()
        {
            try
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetMessage(Resource.String.LicenseFailedMessageString);
                builder.SetPositiveButton(GetString(Resource.String.WriteString), delegate
                {
                    try
                    {
                        var emailIntent = new Intent(Intent.ActionSendto, Uri.Parse("mailto:fernandmobileboardgames@gmail.com"));
                        emailIntent.PutExtra(Intent.ExtraSubject, "Ошибка лицензии");
                        StartActivity(Intent.CreateChooser(emailIntent, string.Empty));
                    }
                    catch (Exception exception)
                    {
                        GaService.TrackAppException(this.Class, "SendEmail", exception, false);
                        var errorBuilder = new AlertDialog.Builder(this);
                        errorBuilder.SetMessage(GetString(Resource.String.SendEmailFailedHeaderString)
                                                + System.Environment.NewLine
                                                + GetString(Resource.String.SendEmailFailedString));
                        errorBuilder.SetCancelable(false);
                        errorBuilder.SetPositiveButton("Ok", (sender, args) => base.OnBackPressed());
                        errorBuilder.Show();
                    }
                   
                });
                builder.SetNegativeButton("Ok", (sender, args) => base.OnBackPressed());
                builder.SetCancelable(false);
                return builder;
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "GetDialog", exception, false);
                throw;
            }
        }
        private void InitLicence()
        {
            _pref1 = GetSharedPreferences(Base64.Encode("RWZmZHNNkeHY"),FileCreationMode.Private);
            _pref2 = Xamarin.InAppBilling.Utilities.Security.Unify(new[] { "dW58z", "ZnGZ" }, new[] { 0,1 });
            var s = _pref1.GetString(_pref2, string.Empty);
            if (string.IsNullOrEmpty(s)) 
                CheckLicense();
            else if (string.Compare(Base64.Encode(true.ToString()), s, StringComparison.OrdinalIgnoreCase) == 0)
                OnLicenseChecked();
            else if (string.Compare(Base64.Encode(false.ToString()), s, StringComparison.OrdinalIgnoreCase) == 0)
                GetDialog().Show();
            else CheckLicense();
        }

        private void OnLicenseChecked()
        {
            try
            {
                InitConnectToGoogleStore();
                _serviceConnection.Connect();
                CreateLocalDatabase();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "OnLicenseChecked", exception, true);
            }
        }

        private async void DatabaseHelperOnDatabaseCreatedOrOpened(object sender, EventArgs eventArgs)
        {
            try
            {
                await Task.Delay(1000);
                if (!_serviceConnection.Connected || _serviceConnection.BillingHandler == null)
                {
                    RunOnUiThread(() => Toast.MakeText(this, "Нет подключения к Google Store", ToastLength.Short));
                }
                else
                {
                    await RefreshPurchaseItems();
                }
                await StartActivityAsync();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "DatabaseHelperOnDatabaseCreatedOrOpened", exception, true);
            }
        }

        private void InitConnectToGoogleStore()
        {
            try
            {
                var key = Xamarin.InAppBilling.Utilities.Security.Unify(new[] {
					GetNumberString (3),
					GetNumberString (6),
					GetNumberString (1),
					GetNumberString (4),
					GetNumberString (2),
					GetNumberString (7),
					GetNumberString (0),
					GetNumberString (5)
				}, new[] { 0, 1, 2, 3, 4, 5, 6, 7 });
                _serviceConnection = new InAppBillingServiceConnection(this, key);
                _serviceConnection.BindErrors();
                _serviceConnection.OnConnected += _serviceConnection_OnConnected;
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.Name, "InitConnectToGoogleStore", exception, false);
            }
        }

        void _serviceConnection_OnConnected()
        {
            if (_serviceConnection.BillingHandler != null)
                _serviceConnection.BillingHandler.BindErrors();
        }

        private async Task StartActivityAsync()
        {
            try
            {
                if (_startActivity)
                    return;
                _startActivity = true;
                var playerIsGenerated = await Rep.Instance.InitPlayersAsync();
                var intent = GetIntentStartActivity(!playerIsGenerated);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "StartActivityAsync", exception, true);
            }
        }

        private Intent GetIntentStartActivity(bool isDefault)
        {
            try
            {
                Rep.Instance.Players.Enumerator.SetCurrent(isDefault ? 0 : this.GetCurrentPlayer());
                if (isDefault)
                {
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
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "GetIntentStartActivity", exception, false);
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
                GaService.TrackAppException(this.Class, "CreateLocalDatabase", ex, false);
            }
        }

        private string GetNumberString(int num)
        {
            var s = new[] {
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
                var purchased = _serviceConnection.BillingHandler.GetPurchases(ItemType.Product);
                var ershPurchases = purchased == null ? new List<ErshPurchase>() : purchased.Where(purchase => purchase.PurchaseState == 0).Select(purchase => PurchaseCreator.Create(purchase.ProductId)).ToList();
                await Rep.DatabaseHelper.CheckAndRefreshPurshases(ershPurchases, _serviceConnection.Connected);
            }
            catch (Exception ex)
            {
                GaService.TrackAppException(this.Class, "RefreshPurchaseItems", ex, false);
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
            if (Rep.DatabaseHelper!=null) Rep.DatabaseHelper.DataBaseCreatedOrOpened -= DatabaseHelperOnDatabaseCreatedOrOpened;
            if (_serviceConnection!= null)
            {
                _serviceConnection.OnConnected -= _serviceConnection_OnConnected;
                _serviceConnection.UnbindErrors();

                if (_serviceConnection.Connected) _serviceConnection.Disconnect();
                if (_serviceConnection.BillingHandler != null) _serviceConnection.BillingHandler.UnbindErrors();
            }
            base.OnDestroy();
        }

    }
}
