using System.Threading.Tasks;
using System;
using Java.Lang;
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
using Yorsh.Model.EventAgruments;
using Exception = System.Exception;

namespace Yorsh.Activities
{
    [Activity(Label = "@string/BuyString", ParentActivity = typeof(MainMenuActivity), MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait)]
    public class StoreActivity : BaseActivity
    {
        private InAppBillingHandler _billingHandler;
        private IEnumerable<ErshProduct> _allErshProducts;
        private InAppBillingServiceConnection _serviceConnection;
        private IList<ErshPurchase> _purchases;
        private ListView _taskListView;
        private ListView _bonusListView;
        private bool _navigatedFromGameActivity;
        private IList<Product> _allProducts;

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

        void ConnectToService()
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
                GaService.TrackAppException(this.Class, "ConnectToService", ex, false);
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


        private async void HandleOnConnected()
        {
            try
            {
                _billingHandler = _serviceConnection.BillingHandler;
                if (_billingHandler == null)
                {
                    ErrorOccur("��� �����������");
                    return; 
                }
                _billingHandler.OnProductPurchased += BillingHandlerOnProductPurchased;
                _billingHandler.BuyProductError += BillingHandler_BuyProductError;
                Rep.DatabaseHelper.DatabaseChanged += InstanceOnDatabaseChanged;
                await GetInventory();
                if (_allErshProducts == null|| !_allErshProducts.Any()|| _allProducts == null || !_allProducts.Any())
                {
                    ErrorOccur("��� �����������");
                    return;
                }
                SetupInventory();
            }
            catch (Exception ex)
            {
                GaService.TrackAppException(this.Class, "HandleOnConnected", ex, true);
                ErrorOccur(ex.Message);
            }
        }

        private IEnumerable<string> GetIds(IEnumerable<int> counts, string type)
        {
            foreach (var oneCount in counts.Select(count => string.Format(@"{0}_{1}", count, type)))
            {
                yield return oneCount;
                yield return oneCount + '_' + 3;
                yield return oneCount + '_' + 4;
            }

            yield return "all_" + type;
        }
        private IEnumerable<string> GetErshIds(IEnumerable<int> counts, string type)
        {
            var list = counts.Select(count => string.Format(@"{0}_{1}", count, type)).ToList();
            list.Add("all_" + type);
            return list;
        }

        private async Task GetInventory()
        {
            try
            {
                var ershTaskIds = GetErshIds(new[] { 10, 30, 70, 100 }, StringConst.Task).ToList();
                var ershBonusIds = GetErshIds(new[] { 10, 30 }, StringConst.Bonus).ToList();
                var allErshProducts = await _billingHandler.QueryInventoryAsync(ershTaskIds.Union(ershBonusIds).ToList(), ItemType.Product);
                _allErshProducts = allErshProducts == null ? null : allErshProducts.Select(product => new ErshProduct(product)).ToList();

                var taskIds = GetIds(new[] { 10, 30, 70, 100 }, StringConst.Task).ToList();
                var bonusIds = GetIds(new[] { 10, 30 }, StringConst.Bonus).ToList();
                var allIds = taskIds.Union(bonusIds).ToList();
                var allProducts = await _billingHandler.QueryInventoryAsync(allIds, ItemType.Product);
                _allProducts =allProducts==null?null:allProducts.ToList();
            }
            catch (Exception ex)
            {
                GaService.TrackAppException(this.Class, "GetInventory", ex, true);
            }

        }
        private void RefreshPurchasedItems()
        {
            try
            {
                var purchases = _billingHandler.GetPurchases(ItemType.Product);
                _purchases = purchases == null
                    ? new List<ErshPurchase>()
                    : purchases.Where(purchase => purchase.PurchaseState == 0).Select(purchase => PurchaseCreator.Create(purchase.ProductId)).ToList();

            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "RefreshPurchasedItem", exception, false);
            }

        }
        void InstanceOnDatabaseChanged(object sender, EventArgs eventArgs)
        {
            SetupInventory();
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
                    //TODO::Send event or error
                    //int result;
                    //var split = sku.Split('_');
                    //if (split.Count() != 2 || !int.TryParse(split[0], out result)) break;
                    //var product = _allErshProducts.FirstOrDefault(p => p.ProductId == sku) ?? _bonusProducts.FirstOrDefault(p => p.ProductId == sku);
                    //var purchase = _purchases.FirstOrDefault(p => p.ProductId == sku);
                    //if (purchase != null && product != null)
                    //{
                    //    message = "�� ������� ������ " + product.Description + "." + System.Environment.NewLine + "���������� �����";
                    //    _billingHandler.ConsumePurchase(purchase);
                    //}
                    break;
            }
            if (string.IsNullOrEmpty(message)) message = "��������� ���-�� ��������������, ��������� ���� ������� �����";
            Toast.MakeText(this, message, ToastLength.Short).Show();
        }

        private async void BillingHandlerOnProductPurchased(int response, Purchase purchase, string purchaseData, string purchaseSignature)
        {
            var ershPurchase = PurchaseCreator.Create(purchase.ProductId);
            if (!ershPurchase.IsAll && GetPurchasesContainesKey(ershPurchase.OrdiginalId).Count() + 1 == GetProductsContainesKey(ershPurchase.OrdiginalId).Count())
                    GaService.TrackAppEvent(StringConst.TrackEventCardsAreEnds, string.Format("{0}", ershPurchase.OrdiginalId));
            
            await Rep.DatabaseHelper.AddProductAsync(ershPurchase);

        }

        private void SetDescriptionText()
        {
            try
            {
                var taskDescription = FindViewById<TextView>(Resource.Id.taskDescription);
                var taskCount = Rep.DatabaseHelper.Tasks.Count;
                var allTaskCount = IntConst.AllTaskCount - Rep.DatabaseHelper.Tasks.Count;
                taskDescription.Text = _purchases.Any(x => x.IsAll && x.ProductType == StringConst.Task)
                    ? string.Format("��� �������� ��� {0} ������� � ����������� �������� �� �����",
                        IntConst.AllTaskCount)
                    : string.Format("� ��� {0} �������, {1}", taskCount,
                        allTaskCount == 0 ? "�� ������ ������ ����������� �������� �� �����" : "����� ������ ��� " + allTaskCount);

                var bonusCount = Rep.DatabaseHelper.Bonuses.Count;
                var allBonusCount = IntConst.AllBonusCount - Rep.DatabaseHelper.Bonuses.Count;
                var bonusDescription = FindViewById<TextView>(Resource.Id.bonusDescription);
                bonusDescription.Text = _purchases.Any(x => x.IsAll && x.ProductType == StringConst.Bonus)
                    ? string.Format("��� �������� ��� {0} ������� � ����������� �������� �� �����",
                        IntConst.AllBonusCount)
                    : string.Format("� ��� {0} �������, {1}", bonusCount,
                    allBonusCount == 0 ? "�� ������ ������ ����������� �������� �� �����" : "����� ������ ��� " + allBonusCount);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "SetDescriptionText", exception, false);
            }

        }

        private void SetUpProducts()
        {
            try
            {
                foreach (var product in _allErshProducts)
                {
                    string enabledProductId;
                    product.IsEnabled = ProductIsEnabled(product, out enabledProductId);
                    product.EnableProductId = enabledProductId;
                }
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "SetUpProducts", exception, false);
            }

        }

        private void SetupInventory()
        {
            try
            {
                RefreshPurchasedItems();
                SetDescriptionText();
                SetUpProducts();

                _taskListView = FindViewById<ListView>(Resource.Id.taskListView);
                var adapter = new StoreListAdapter(this, _allErshProducts.Where(product=>product.Type == StringConst.Task));
                adapter.ItemClick += AdapterOnItemClick;
                _taskListView.Adapter = new MultiItemRowListAdapter(this, adapter, 3, 1);
                _taskListView.JustifyListViewHeightBasedOnChildren();

                _bonusListView = FindViewById<ListView>(Resource.Id.bonusListView);
                var bonusAdapter = new StoreListAdapter(this, _allErshProducts.Where(product => product.Type == StringConst.Bonus));
                bonusAdapter.ItemClick += AdapterOnItemClick;
                _bonusListView.Adapter = new MultiItemRowListAdapter(this, bonusAdapter, 3, 1);
                _bonusListView.JustifyListViewHeightBasedOnChildren();


                FindViewById<ScrollView>(Resource.Id.googleStoreActive).Visibility = ViewStates.Visible;
                FindViewById<TextView>(Resource.Id.googleStoreNotActive).Visibility = ViewStates.Gone;
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "SetupInventory", exception, false);
            }

        }
        private void AdapterOnItemClick(object sender, StoreItemClickEventArgs e)
        {
            var buyProduct = _allProducts.FirstOrDefault(product => string.CompareOrdinal(product.ProductId, e.Product.EnableProductId) == 0);
            if (buyProduct!=null) _billingHandler.BuyProduct(buyProduct);
        }

        private bool ContainAllPurchase(string type)
        {
            return _purchases.Any(pur => pur.IsAll && pur.ProductType == type);
        }

        private IDictionary<string, ErshPurchase> GetPurchasesContainesKey(string productId)
        {
            return _purchases.Where(purchase => purchase.PurchaseId.Contains(productId)).ToDictionary(ershPurchase => ershPurchase.PurchaseId);
        }
        private IEnumerable<Product> GetProductsContainesKey(string productId)
        {
            return _allProducts.Where(prodT => prodT.ProductId.Contains(productId)).ToList();
        }

        private bool CanAddToDataBase(string productId)
        {
            int result;
            var isCount = int.TryParse(productId.Split('_')[0], out result);
            var count = productId.Split('_')[1] == StringConst.Bonus ? Rep.DatabaseHelper.Bonuses.Count : Rep.DatabaseHelper.Tasks.Count;
            var allCount = productId.Split('_')[1] == StringConst.Bonus ? IntConst.AllBonusCount : IntConst.AllTaskCount;

            return !isCount || allCount - count >= result;
        }

        private bool ProductIsEnabled(ErshProduct product, out string enableProductId)
        {
            enableProductId = string.Empty;
            if (ContainAllPurchase(product.Type) || !CanAddToDataBase(product.ProductId)) return false;

            var purchases = GetPurchasesContainesKey(product.ProductId);
            var allProducts = GetProductsContainesKey(product.ProductId);
            foreach (var allProd in allProducts)
            {
                ErshPurchase purchase;
                if (purchases.TryGetValue(allProd.ProductId, out purchase)) continue;
                enableProductId = allProd.ProductId;
                return true;
            }

            return false;
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