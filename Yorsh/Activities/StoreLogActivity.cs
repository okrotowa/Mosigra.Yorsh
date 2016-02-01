using System.IO;
using System.Threading.Tasks;
using System;
using Java.Interop;
using Yorsh.Adapters;
using Yorsh.Helpers;
using System.Collections.Generic;
using Yorsh.Model;
using Android.App;
using Xamarin.InAppBilling;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Content.PM;
using System.Linq;
using Android.Views;
using Java.IO;
using File = Java.IO.File;
using System.Text;

namespace Yorsh.Activities
{
    [Activity(Label = "@string/BuyString", ParentActivity = typeof(MainMenuActivity), MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait)]
    public class StoreLogActivity : BaseActivity
    {
        private InAppBillingHandler _billingHandler;
        private IList<Product> _taskProducts;
        private IList<Product> _bonusProducts;
        private InAppBillingServiceConnection _serviceConnection;
        private IList<Purchase> _purchases;
        private ListView _taskListView;
        private ListView _bonusListView;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Store);

            ErrorOccur("Подключение к Google Store");
            FindViewById<TextView>(Resource.Id.taskHeader).SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);
            var taskDescription = FindViewById<TextView>(Resource.Id.taskDescription);
            taskDescription.SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed),
                Android.Graphics.TypefaceStyle.Bold);

            var googleStoreNotActive = FindViewById<TextView>(Resource.Id.googleStoreNotActive);
            googleStoreNotActive.SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);
            googleStoreNotActive.Clickable = true;
            googleStoreNotActive.Click += (sender, e) =>
            {
                googleStoreNotActive.Text = string.Empty;
                googleStoreNotActive.Visibility = ViewStates.Gone;
                FindViewById<ScrollView>(Resource.Id.googleStoreActive).Visibility = ViewStates.Visible;
            };
            FindViewById<TextView>(Resource.Id.bonusHeader)
                .SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);
            var bonusDescription = FindViewById<TextView>(Resource.Id.bonusDescription);
            bonusDescription.SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed),
                Android.Graphics.TypefaceStyle.Bold);

            //FindViewById<TextView>(Resource.Id.moreHeader)
            //    .SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);
            //var moreDescription = FindViewById<TextView>(Resource.Id.moreDescription);
            //moreDescription.SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed),
            //    Android.Graphics.TypefaceStyle.Bold);

            //var morePriceText = FindViewById<TextView>(Resource.Id.morePriceText);
            //morePriceText.SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed),
            //    Android.Graphics.TypefaceStyle.Normal);


            ConnectToService();
        }

        void InstanceOnDatabaseChanged(object sender, EventArgs eventArgs)
        {
            SetupInventory();
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

        protected override void OnDestroy()
        {
            if (_serviceConnection != null && _serviceConnection.Connected)
            {
                _serviceConnection.Disconnect();
            }

            if (_billingHandler != null)
            {
                _billingHandler.OnProductPurchased -= BillingHandlerOnProductPurchased;
                _billingHandler.OnProductPurchasedError -= BillingHandlerOnProductPurchasedError;
                _billingHandler.BuyProductError -= _billingHandler_BuyProductError;
                Rep.Instance.DatabaseChanged -= InstanceOnDatabaseChanged;

            }
            Toast.MakeText(this, "Destroyed", ToastLength.Short).Show();
            base.OnDestroy();
        }

        private void RefreshPurchasedItems()
        {
            _purchases = _billingHandler.GetPurchases(ItemType.Product);
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Purchases: ");
            foreach (var purchase in _purchases)
            {
                stringBuilder.AppendLine(string.Format("id={0},state={1},tocken={2},devel={3},order={4}", purchase.ProductId, purchase.PurchaseState, purchase.PurchaseToken, purchase.DeveloperPayload, purchase.OrderId));
            }
            ErrorOccur(stringBuilder.ToString());
        }

        private void ConnectToService()
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
            _serviceConnection.OnConnected += HandleOnConnected;
            _serviceConnection.OnInAppBillingError += ServiceConnectionOnInAppBillingError;
            _serviceConnection.Connect();
        }

        private async void HandleOnConnected()
        {
            try
            {
                Toast.MakeText(this, "Произошло подключение", ToastLength.Short).Show();
                _billingHandler = _serviceConnection.BillingHandler;
                if (_billingHandler == null)
                    return;
                _billingHandler.OnProductPurchased += BillingHandlerOnProductPurchased;
                _billingHandler.OnProductPurchasedError += BillingHandlerOnProductPurchasedError;
                _billingHandler.BuyProductError += _billingHandler_BuyProductError;
                _billingHandler.InAppBillingProcesingError += (message) => { ErrorOccur("InAppBillingProcesingError: " + message); };
                _billingHandler.OnGetProductsError += (int responseCode, Bundle ownedItems) => { ErrorOccur("OnGetProductsError: " + responseCode); };
                _billingHandler.OnInvalidOwnedItemsBundleReturned += (Bundle ownedItems) => { ErrorOccur("OnInvalidOwnedItemsBundleReturned"); };
                _billingHandler.OnPurchaseConsumedError += (int responseCode, string token) => { ErrorOccur("OnPurchaseConsumedError: token=" + token); };
                _billingHandler.OnPurchaseFailedValidation += (Purchase purchase, string purchaseData, string purchaseSignature)
                    => { ErrorOccur(string.Format("OnPurchaseFailedValidation: id={0}, state={1}", purchase.ProductId, purchase.PurchaseState)); };
                Rep.Instance.DatabaseChanged += InstanceOnDatabaseChanged;
                await GetInventory();
                SetupInventory();
            }
            catch (Exception ex)
            {
                GaService.TrackAppException(ex.Message, true);
                ErrorOccur(ex.Message);
            }
        }

        void _billingHandler_BuyProductError(int responseCode, string sku)
        {
            ErrorOccur("Buy product error: " + sku);
        }

        private void ServiceConnectionOnInAppBillingError(InAppBillingErrorType error, string message)
        {
            var message1 = string.Format("Store(ServiceConnectionOnInAppBillingError)/nerror={0}, message = {1}", error, message);
            GaService.TrackAppException(message1, false);
            ErrorOccur("Error of service connection: " + message);
        }

        private void BillingHandlerOnProductPurchasedError(int responseCode, string sku)
        {
            var message = string.Format("Store(BillingHandlerOnOnProductPurchasedError)/nresponse code={0}, sku = {1}",
                responseCode, sku);
            GaService.TrackAppException(message, false);
            ErrorOccur("Purchase error: " + sku);
        }

        private void BillingHandlerOnProductPurchased(int response, Purchase purchase, string purchaseData, string purchaseSignature)
        {
            ErrorOccur(string.Format("It work{0}, id={1}, state={2}", System.Environment.NewLine, purchase.ProductId, purchase.PurchaseState));
            this.AddProduct(purchase.ProductId).ContinueWith(delegate
            {
                int result;
                if (int.TryParse(purchase.ProductId.Split('_')[0], out result))
                {
                    _billingHandler.ConsumePurchase(purchase);
                }
            });
        }

        private void SetDescriptionText()
        {
            Toast.MakeText(this, "SetDescriptionText", ToastLength.Short).Show();
            var taskDescription = FindViewById<TextView>(Resource.Id.taskDescription);
            var taskCount = Rep.Instance.Tasks.Count;
            var allTaskCount = Rep.Instance.AllTaskCount - Rep.Instance.Tasks.Count;
            taskDescription.Text = _purchases.Any(x => string.CompareOrdinal(x.ProductId, "all_task") == 0)
                ? string.Format("Вам доступны все {0} заданий и безлимитная подписка на новые",
                    Rep.Instance.AllTaskCount)
                : string.Format("У вас {0} заданий, {1}", taskCount,
                    allTaskCount == 0 ? "вы можете купить безлимитную подписку на новые" : "можно купить еще " + allTaskCount);

            var bonusCount = Rep.Instance.Bonuses.Count;
            var allBonusCount = Rep.Instance.AllBonusCount - Rep.Instance.Bonuses.Count;
            var bonusDescription = FindViewById<TextView>(Resource.Id.bonusDescription);
            bonusDescription.Text = _purchases.Any(x => string.CompareOrdinal(x.ProductId, "all_bonus") == 0)
                ? string.Format("Вам доступны все {0} бонусов и безлимитная подписка на новые",
                    Rep.Instance.AllBonusCount)
                : string.Format("У вас {0} бонусов, {1}", bonusCount,
                allBonusCount == 0 ? "вы можете купить безлимитную подписку на новые" : "можно купить еще " + allBonusCount);
        }

        private void SetupInventory()
        {
            RefreshPurchasedItems();
            SetDescriptionText();

            //FindViewById<ScrollView>(Resource.Id.googleStoreActive).Visibility = ViewStates.Visible;
            //FindViewById<TextView>(Resource.Id.googleStoreNotActive).Visibility = ViewStates.Gone;
            _taskListView = FindViewById<ListView>(Resource.Id.taskListView);
            var adapter = new StoreListAdapter(this, _taskProducts, GetSaleForTask, TaskIsEnabled);
            adapter.ItemClick += AdapterOnItemClick;
            _taskListView.Adapter = new MultiItemRowListAdapter(this, adapter, 3, 1);
            _taskListView.JustifyListViewHeightBasedOnChildren();
            _bonusListView = FindViewById<ListView>(Resource.Id.bonusListView);

            var bonusAdapter = new StoreListAdapter(this, _bonusProducts, GetSaleForBonus, BonusIsEnabled);
            bonusAdapter.ItemClick += AdapterOnItemClick;
            _bonusListView.Adapter = new MultiItemRowListAdapter(this, bonusAdapter, 3, 1);
            _bonusListView.JustifyListViewHeightBasedOnChildren();
        }

        private int GetSaleForBonus(string id)
        {
            var splitArgs = id.Split('_');
            int count;
            if (splitArgs.Count() != 2 || !int.TryParse(splitArgs[0], out count)) return 0;
            switch (count)
            {
                case 30:
                    return 25;
                default:
                    return 0;
            }
        }

        private int GetSaleForTask(string id)
        {
            var splitArgs = id.Split('_');
            int count;
            if (splitArgs.Count() != 2 || !int.TryParse(splitArgs[0], out count)) return 0;
            switch (count)
            {
                case 30:
                    return 25;
                case 70:
                    return 50;
                case 100:
                    return 70;
                default:
                    return 0;
            }
        }

        private void AdapterOnItemClick(object sender, StoreItemClickEventArgs e)
        {
            Toast.MakeText(this, e.Product.ProductId, ToastLength.Short).Show();
            _billingHandler.BuyProduct(e.Product);
        }

        private bool TaskIsEnabled(string productId)
        {
            var purchase = _purchases.FirstOrDefault(x => string.CompareOrdinal(x.ProductId, "all_task") == 0);
            if (purchase != null && (purchase.PurchaseState == BillingResult.OK || purchase.PurchaseState == BillingResult.ItemAlreadyOwned)) return false;
            int result;
            var prod = productId.Split('_');
            var taskCount = Rep.Instance.Tasks.Count;
            var enabled = !int.TryParse(prod[0], out result) || Rep.Instance.AllTaskCount - taskCount >= result;
            return enabled;
        }

        private bool BonusIsEnabled(string productId)
        {
            var purchase = _purchases.FirstOrDefault(x => string.CompareOrdinal(x.ProductId, "all_bonus") == 0);
            if (purchase != null && (purchase.PurchaseState == BillingResult.OK || purchase.PurchaseState == BillingResult.ItemAlreadyOwned)) return false;
            int result;
            var prod = productId.Split('_');
            var bonusCount = Rep.Instance.Bonuses.Count;
            return !int.TryParse(prod[0], out result) || Rep.Instance.AllBonusCount - bonusCount >= result;
        }

        private async Task GetInventory()
        {
            try
            {
                _taskProducts = await _billingHandler.QueryInventoryAsync(new List<string>
                {
                    "10_task",
                    "30_task",
                    "70_task",
                    "100_task",
                    "all_task",
                }, ItemType.Product);

                var stringBuilder = new StringBuilder();
                stringBuilder.Append("Task Products: ");
                foreach (var task in _taskProducts)
                {
                    stringBuilder.Append(task.ProductId + " ");
                }

                _bonusProducts = await _billingHandler.QueryInventoryAsync(new List<string>()
                {
                    "10_bonus",
                    "30_bonus",
                    "all_bonus"
                }, ItemType.Product);
                stringBuilder.AppendLine("Bonus Products: ");
                foreach (var bonus in _bonusProducts)
                {
                    stringBuilder.Append(bonus.ProductId + " ");
                }

                ErrorOccur(stringBuilder.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("GetInventory: " + ex.Message);
            }

        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode != Keycode.Back) return base.OnKeyDown(keyCode, e);
            OnBackPressed();
            return false;
        }

        private void GetStubInventory()
        {
            //Get available products
            _taskProducts = new[]
            {
                new Product() {Price = "24", ProductId = "10_task", Price_Currency_Code = "RUB"},
                new Product() {Price = "26", ProductId = "30_task", Price_Currency_Code = "RUB"},
                new Product() {Price = "46", ProductId = "70_task", Price_Currency_Code = "RUB"},
                new Product() {Price = "67", ProductId = "100_task", Price_Currency_Code = "RUB"},
                new Product() {Price = "1468", ProductId = "all_task", Price_Currency_Code = "RUB"}
            };

            _bonusProducts = new[]
            {
                new Product() {Price = "34", ProductId = "10_bonus", Price_Currency_Code = "RUB"},
                new Product() {Price = "67", ProductId = "30_bonus", Price_Currency_Code = "RUB"},
                new Product() {Price = "236", ProductId = "all_bonus", Price_Currency_Code = "RUB"}
            };
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            Toast.MakeText(this, string.Format("ActivityResults: rc = {0}, RESULTS= {1}", resultCode, resultCode), ToastLength.Long).Show();
            _billingHandler.HandleActivityResult(requestCode, resultCode, data);
        }

        private void ErrorOccur(string message)
        {
            FindViewById<ScrollView>(Resource.Id.googleStoreActive).Visibility = ViewStates.Gone;
            var googleStoreNotActive = FindViewById<TextView>(Resource.Id.googleStoreNotActive);
            googleStoreNotActive.Visibility = ViewStates.Visible;
            googleStoreNotActive.Text += string.IsNullOrEmpty(googleStoreNotActive.Text) ? message : System.Environment.NewLine + message;
        }
    }


}