using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Content.PM;
using Xamarin.InAppBilling;
using Android.Widget;
using System;
using Android.Views;
using Yorsh.Adapters;
using Yorsh.Helpers;
using System.Collections.Generic;
using Android.Content;
using Yorsh.Model;

namespace Yorsh.Activities
{
    [Activity(Label = "@string/BuyString", ParentActivity = typeof(MainMenuActivity), MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait)]
    public class StoreActivity : BaseActivity
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

            //FindViewById<TextView>(Resource.Id.moreHeader)
            //    .SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);
            //var moreDescription = FindViewById<TextView>(Resource.Id.moreDescription);
            //moreDescription.SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed),
            //    Android.Graphics.TypefaceStyle.Bold);

            //var morePriceText = FindViewById<TextView>(Resource.Id.morePriceText);
            //morePriceText.SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed),
            //    Android.Graphics.TypefaceStyle.Normal);

            StartSetup();
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
            if (_serviceConnection != null && _serviceConnection.BillingHandler != null)
            {
                _serviceConnection.Disconnect();
                _billingHandler.OnProductPurchased -= BillingHandlerOnOnProductPurchased;
                _billingHandler.OnProductPurchasedError -= BillingHandlerOnOnProductPurchasedError;
            }
            base.OnDestroy();
        }

        private void GetPurchasedItems()
        {
            _purchases = _billingHandler.GetPurchases(ItemType.Product);
            SetDescriptionText();
        }

        private void UpdateStubPurchasedItems()
        {
            _purchases = new List<Purchase> { new Purchase() { ProductId = "10_task", PurchaseState = 0 } };
        }

        private void StartSetup()
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
            _serviceConnection.Connect();

        }

        private async void HandleOnConnected()
        {
            try
            {
                _serviceConnection.OnInAppBillingError += ServiceConnectionOnOnInAppBillingError;
                _billingHandler = _serviceConnection.BillingHandler;
                _billingHandler.OnProductPurchased += BillingHandlerOnOnProductPurchased;
                _billingHandler.OnProductPurchasedError += BillingHandlerOnOnProductPurchasedError;
                
                GetPurchasedItems();
                await GetInventory();
                SetupInventory();

                Rep.Instance.DatabaseChanged += InstanceOnDatabaseChanged;
            }
            catch (Exception ex)
            {
                ErrorOccur(ex.Message + "\n" + ex.InnerException);
            }

        }

        private void ServiceConnectionOnOnInAppBillingError(InAppBillingErrorType error, string message)
        {
            ErrorOccur(error + "\n" + message);
        }

        private void BillingHandlerOnOnProductPurchasedError(int responseCode, string sku)
        {
            ErrorOccur("Что-то пошло не так во время покупки:(");
        }

        private async void BillingHandlerOnOnProductPurchased(int response, Purchase purchase, string purchaseData, string purchaseSignature)
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
            var count = Rep.Instance.Tasks.Count;
            var allCount = Rep.Instance.AllTaskCount - Rep.Instance.Tasks.Count;
            taskDescription.Text = _purchases.Any(x => string.CompareOrdinal(x.ProductId, "all_task") == 0)
                ? string.Format("Вам доступны все {0} заданий и безлимитная подписка на новые",
                    Rep.Instance.AllTaskCount)
                : string.Format("У вас {0} заданий, {1}", count,
                    allCount == 0 ? "вы можете купить безлимитную подписку на новые" : "можно купить еще " + allCount);

            count = Rep.Instance.Bonuses.Count;
            allCount = Rep.Instance.AllBonusCount - Rep.Instance.Bonuses.Count;
            var bonusDescription = FindViewById<TextView>(Resource.Id.bonusDescription);
            bonusDescription.Text = _purchases.Any(x => string.CompareOrdinal(x.ProductId, "all_bonus") == 0)
                ? string.Format("Вам доступны все {0} бонусов и безлимитная подписка на новые",
                    Rep.Instance.AllBonusCount)
                : string.Format("У вас {0} бонусов, {1}", count,
                allCount == 0 ? "вы можете купить безлимитную подписку на новые" : "можно купить еще " + allCount);
        }

        private void SetupInventory()
        {
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

        private void AdapterOnItemClick(object sender, StoreItemClickEventArgs e)
        {
            _billingHandler.BuyProduct(e.Product);
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

        private async Task GetInventory()
        {
            try
            {
                //Get available products
                _taskProducts = await _billingHandler.QueryInventoryAsync(new List<string>
            {
                "10_task",
                "30_task",
                "70_task",
                "100_task",
                "all_task"
            }, ItemType.Product);

                _bonusProducts = await _billingHandler.QueryInventoryAsync(new List<string>()
            {
                "10_bonus",
                "30_bonus",
                "all_bonus"
            }, ItemType.Product);


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
            _billingHandler.HandleActivityResult(requestCode, resultCode, data);
            GetPurchasedItems();
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