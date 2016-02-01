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

namespace Yorsh.Activities
{
    [Activity(Label = "@string/BuyString", ParentActivity = typeof(MainMenuActivity), ScreenOrientation = ScreenOrientation.Portrait)]
    public class StoreStubActivity : BaseActivity
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

            FindViewById<TextView>(Resource.Id.googleStoreNotActive)
                .SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);

            FindViewById<TextView>(Resource.Id.bonusHeader)
                .SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);
            var bonusDescription = FindViewById<TextView>(Resource.Id.bonusDescription);
            bonusDescription.SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed),
                Android.Graphics.TypefaceStyle.Bold);

            Rep.Instance.DatabaseChanged += InstanceOnDatabaseChanged;
            ConnectToService();
        }

        private void InstanceOnDatabaseChanged(object sender, EventArgs eventArgs)
        {
            SetupInventory();
        }

        private string GetNumberString(int num)
        {
            var m = new[]
            {
                @"2UN43ovQsqAfb1WNQ+qvc8aVwNq0BXiVmNecqLYbsL2jZSGS7o",
                @"IpiUY9k8qeqHeZkrTVMi//AbuWK7Hx+4hhf6gIR4U4Mz544aXX93w",
                @"MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAr1ws",
                @"x/Y8Xl03wsQfVHoOY6LDORvYN+ZqJxEuS1dqcUul4TiUqbemZKO",
                @"56OTgnC32Ngqoa8RkzHpQPoS5dvLUMMwIDAQAB",
                @"RhQz/aFBC2HGRbIJ0Dx372QXWoSYBZT8nESCUl9dxhILH+dSS",
                @"AqNQeRUmMz/QmCnNiINuaOv3vOQKMyJsYPwqg+9oei/9bYSIPdk",
                @"XiyHATbEIiMULheQrS0w4fMu8Fk6nqAMmp8RAXX/fCDCIoIkJ42k"
            };

            return m[num];
        }

        protected override void OnDestroy()
        {
            if (_serviceConnection != null && _serviceConnection.Connected)
            {
                _serviceConnection.Disconnect();
            }
            base.OnDestroy();
        }

        private void RefreshPurchasedItems()
        {
            _purchases = _purchases ?? new List<Purchase>();
        }

        private void ConnectToService()
        {
            var key = Xamarin.InAppBilling.Utilities.Security.Unify(new[]
                {
                    GetNumberString(2), 
                    GetNumberString(5), 
                    GetNumberString(0), 
                    GetNumberString(3),
                    GetNumberString(6),
                    GetNumberString(7), 
                    GetNumberString(1),
                    GetNumberString(4)
                },
             new[] { 0, 1, 2, 3, 4, 5, 6, 7 }
             );
            _serviceConnection = new InAppBillingServiceConnection(this, key);
            _serviceConnection.OnConnected += HandleOnConnected;
            _serviceConnection.OnInAppBillingError += ServiceConnectionOnInAppBillingError;
            _serviceConnection.Connect();
            HandleOnConnected();
        }

        private async void HandleOnConnected()
        {
            try
            {
                _billingHandler = _serviceConnection.BillingHandler;
                if (_billingHandler != null)
                {
                    _billingHandler.OnProductPurchased += BillingHandlerOnProductPurchased;
                    _billingHandler.OnProductPurchasedError += BillingHandlerOnProductPurchasedError;
                }

                await GetInventory();
                SetupInventory();
            }
            catch (Exception ex)
            {
                GaService.TrackAppException(ex.Message, true);
                ErrorOccur(ex.Message);
            }

        }

        private void ServiceConnectionOnInAppBillingError(InAppBillingErrorType error, string message)
        {
            var message1 = string.Format("Store(ServiceConnectionOnInAppBillingError)/nerror={0}, message = {1}", error, message);
            GaService.TrackAppException(message1, false);
        }

        private void BillingHandlerOnProductPurchasedError(int responseCode, string sku)
        {
            var message = string.Format("Store(BillingHandlerOnOnProductPurchasedError)/nresponse code={0}, sku = {1}",
                responseCode, sku);
            GaService.TrackAppException(message, false);
        }

        private async void BillingHandlerOnProductPurchased(int response, Purchase purchase, string purchaseData, string purchaseSignature)
        {
            int result;
            await this.AddProduct(purchase.ProductId);
            if (int.TryParse(purchase.ProductId.Split('_')[0], out result))
            {
                _billingHandler.ConsumePurchase(purchase);
            }
        }

        private void SetDescriptionText()
        {
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

            FindViewById<ScrollView>(Resource.Id.googleStoreActive).Visibility = ViewStates.Visible;
            FindViewById<TextView>(Resource.Id.googleStoreNotActive).Visibility = ViewStates.Gone;
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

        private async void AdapterOnItemClick(object sender, StoreItemClickEventArgs e)
        {
            //_billingHandler.BuyProduct(e.Product);
            _purchases.Add(new Purchase(){PurchaseState = 0, ProductId = e.Product.ProductId});
            await this.AddProduct(e.Product.ProductId);
        }

        private bool TaskIsEnabled(string productId)
        {
            var purchase = _purchases.FirstOrDefault(x => string.CompareOrdinal(x.ProductId, "all_task") == 0);
            if (purchase != null && (purchase.PurchaseState == BillingResult.OK || purchase.PurchaseState == BillingResult.ItemAlreadyOwned)) return false;
            int result;
            var prod = productId.Split('_');
            var enabled = !int.TryParse(prod[0], out result) || Rep.Instance.AllTaskCount - Rep.Instance.Tasks.Count >= result;
            return enabled;
        }

        private bool BonusIsEnabled(string productId)
        {
            var purchase = _purchases.FirstOrDefault(x => string.CompareOrdinal(x.ProductId, "all_bonus") == 0);
            if (purchase != null && (purchase.PurchaseState == BillingResult.OK || purchase.PurchaseState == BillingResult.ItemAlreadyOwned)) return false;
            int result;
            var prod = productId.Split('_');
            return !int.TryParse(prod[0], out result) || Rep.Instance.AllBonusCount - Rep.Instance.Bonuses.Count >= result;
        }

        private async Task<Task> GetInventory()
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    _taskProducts = new List<Product>();
                    var tasks = new List<string>
                    {
                        "10_task",
                        "30_task",
                        "70_task",
                        "100_task",
                        "all_task",
                    };

                    foreach (var taskProduct in tasks)
                    {
                        _taskProducts.Add(new Product() { ProductId = taskProduct, Price = "20" });
                    }

                    _bonusProducts = new List<Product>();
                    var bonuses = new List<string>()
                    {
                        "10_bonus",
                        "30_bonus",
                        "all_bonus"
                    }
                        ;
                    foreach (var bonuse in bonuses)
                    {
                        _bonusProducts.Add(new Product() { ProductId = bonuse, Price = "20" });
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("GetInventory: " + ex.Message);
                }
            }, TaskCreationOptions.None);
        }
        
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            _billingHandler.HandleActivityResult(requestCode, resultCode, data);
        }

        private void ErrorOccur(string message)
        {
            FindViewById<ScrollView>(Resource.Id.googleStoreActive).Visibility = ViewStates.Gone;
            var googleStoreNotActive = FindViewById<TextView>(Resource.Id.googleStoreNotActive);
            googleStoreNotActive.Visibility = ViewStates.Visible;
            googleStoreNotActive.Text = message;
        }
    }


}