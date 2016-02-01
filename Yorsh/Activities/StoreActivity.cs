using System.Threading.Tasks;
using System;
using Yorsh.Adapters;
using Yorsh.Data;
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
        private bool _navigatedFromGameActivity;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Store);
            _navigatedFromGameActivity = this.Intent.GetBooleanExtra("from_game", false);

            ErrorOccur("����������� � Google Store");
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
                _serviceConnection.OnConnected -= HandleOnConnected;
                _serviceConnection.Disconnect();
            }

            if (_billingHandler != null)
            {
                _billingHandler.OnProductPurchased -= BillingHandlerOnProductPurchased;
                _billingHandler.BuyProductError -= BillingHandler_BuyProductError;
                Rep.DatabaseHelper.DatabaseChanged -= InstanceOnDatabaseChanged;
            }
            base.OnDestroy();
        }

        private void RefreshPurchasedItems()
        {
            try
            {
                var purchases = _billingHandler.GetPurchases(ItemType.Product);
                _purchases = purchases == null
                    ? new List<Purchase>()
                    : purchases.Where(purchase => purchase.PurchaseState == 0).ToList();
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "RefreshPurchasedItem", exception, false);
            }

        }

        private void ConnectToService()
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
                                _serviceConnection.OnConnected += HandleOnConnected;
                                _serviceConnection.Connect();
            }
            catch (Exception ex)
            {
                GaService.TrackAppException(this.Class.SimpleName, "ConnectToService", ex, false);
            }

        }

        private async void HandleOnConnected()
        {
            try
            {
                _billingHandler = _serviceConnection.BillingHandler;
                if (_billingHandler == null)
                    return;
                _billingHandler.OnProductPurchased += BillingHandlerOnProductPurchased;
                _billingHandler.BuyProductError += BillingHandler_BuyProductError;
                Rep.DatabaseHelper.DatabaseChanged += InstanceOnDatabaseChanged;

                await GetInventory();
                if (_taskProducts == null || _bonusProducts == null)
                {
                    ErrorOccur("��� �����������");
                    return;
                }
                SetupInventory();
            }
            catch (Exception ex)
            {
                GaService.TrackAppException(this.Class.SimpleName, "HandleOnConnected", ex, true);
                ErrorOccur(ex.Message);
            }
        }

        void BillingHandler_BuyProductError(int responseCode, string sku)
        {
            string message = string.Empty;
            switch (responseCode)
            {
                case BillingResult.BillingUnavailable:
                case BillingResult.ServiceUnavailable:
                    message = "�� ������ ������ ������ ����������";
                    break;
                case BillingResult.ItemAlreadyOwned:
                    int result;
                    var split = sku.Split('_');
                    if (split.Count() != 2 || !int.TryParse(split[0], out result)) break;
                    var product = _taskProducts.FirstOrDefault(p => p.ProductId == sku) ?? _bonusProducts.FirstOrDefault(p => p.ProductId == sku);
                    var purchase = _purchases.FirstOrDefault(p => p.ProductId == sku);
                    if (purchase != null && product != null)
                    {
                        message = "�� ������� ������ " + product.Description + "." + System.Environment.NewLine + "���������� �����";
                        _billingHandler.ConsumePurchase(purchase);
                    }
                    break;
            }
            if (string.IsNullOrEmpty(message)) message = "��������� ���-�� ��������������, ��������� ���� ������� �����";
            Toast.MakeText(this, message, ToastLength.Short).Show();
        }

        private void BillingHandlerOnProductPurchased(int response, Purchase purchase, string purchaseData, string purchaseSignature)
        {
            var ershPurchase = new ErshPurchase(purchase.ProductId);
            Rep.DatabaseHelper.AddProductAsync(ershPurchase).ContinueWith(t =>
            {
                if (!ershPurchase.IsAll) _billingHandler.ConsumePurchase(purchase);
            });
        }

        private void SetDescriptionText()
        {
            try
            {
                var taskDescription = FindViewById<TextView>(Resource.Id.taskDescription);
                var taskCount = Rep.DatabaseHelper.Tasks.Count;
                var allTaskCount = IntConst.AllTaskCount - Rep.DatabaseHelper.Tasks.Count;
                taskDescription.Text = _purchases.Any(x => string.CompareOrdinal(x.ProductId, "all_task") == 0)
                    ? string.Format("��� �������� ��� {0} ������� � ����������� �������� �� �����",
                        IntConst.AllTaskCount)
                    : string.Format("� ��� {0} �������, {1}", taskCount,
                        allTaskCount == 0 ? "�� ������ ������ ����������� �������� �� �����" : "����� ������ ��� " + allTaskCount);

                var bonusCount = Rep.DatabaseHelper.Bonuses.Count;
                var allBonusCount = IntConst.AllBonusCount - Rep.DatabaseHelper.Bonuses.Count;
                var bonusDescription = FindViewById<TextView>(Resource.Id.bonusDescription);
                bonusDescription.Text = _purchases.Any(x => string.CompareOrdinal(x.ProductId, "all_bonus") == 0)
                    ? string.Format("��� �������� ��� {0} ������� � ����������� �������� �� �����",
                        IntConst.AllBonusCount)
                    : string.Format("� ��� {0} �������, {1}", bonusCount,
                    allBonusCount == 0 ? "�� ������ ������ ����������� �������� �� �����" : "����� ������ ��� " + allBonusCount);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "SetDescriptionText", exception, false);
            }

        }

        private void SetupInventory()
        {
            try
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
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class.SimpleName, "SetupInventory", exception, false);
            }

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
            if (purchase != null) return false;
            int result;
            var prod = productId.Split('_');
            var taskCount = Rep.DatabaseHelper.Tasks.Count;
            var enabled = !int.TryParse(prod[0], out result) || IntConst.AllTaskCount - taskCount >= result;
            return enabled;
        }

        private bool BonusIsEnabled(string productId)
        {
            var purchase = _purchases.FirstOrDefault(x => string.CompareOrdinal(x.ProductId, "all_bonus") == 0);
            if (purchase != null) return false;
            int result;
            var prod = productId.Split('_');
            var bonusCount = Rep.DatabaseHelper.Bonuses.Count;
            return !int.TryParse(prod[0], out result) || IntConst.AllBonusCount - bonusCount >= result;
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
                _bonusProducts = await _billingHandler.QueryInventoryAsync(new List<string>()
                {
                    "10_bonus",
                    "30_bonus",
                    "all_bonus"
                }, ItemType.Product);
            }
            catch (Exception ex)
            {
                GaService.TrackAppException(this.Class.SimpleName, "GetInventory", ex, true);
            }

        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (resultCode == Result.Canceled) return;
            _billingHandler.HandleActivityResult(requestCode, resultCode, data);
        }

        private void ErrorOccur(string message)
        {
            FindViewById<ScrollView>(Resource.Id.googleStoreActive).Visibility = ViewStates.Gone;
            var googleStoreNotActive = FindViewById<TextView>(Resource.Id.googleStoreNotActive);
            googleStoreNotActive.Visibility = ViewStates.Visible;
            googleStoreNotActive.Text = message;
        }

        public override void OnPreBackPressed()
        {
            AllowBackPressed = !_navigatedFromGameActivity;
            if (_navigatedFromGameActivity)
            {
                var gameActivity = new Intent(this, typeof(GameActivity));
                this.StartActivityWithoutBackStack(gameActivity);
            }

        }
    }
}