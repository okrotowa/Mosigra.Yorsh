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
        private TextView _googleStoreNotActive;
        private TextView _taskHeader;
        private TextView _taskDescription;
        private TextView _bonusHeader;
        private TextView _bonusDescription;
        private ScrollView _googleStoreActive;
        private StoreListAdapter _taskListAdapter;
        private StoreListAdapter _bonusListAdapter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Store);
            Initialize();
            RegisterSubscribes();
            ErrorOccur("Подключение к Google Store");
            ConnectToService();
        }

        private void Initialize()
        {
            _navigatedFromGameActivity = this.Intent.GetBooleanExtra("from_game", false);

            _googleStoreNotActive = FindViewById<TextView>(Resource.Id.googleStoreNotActive);
            _googleStoreNotActive.SetTypeface(Rep.FontManager.Get(Font.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);
            _googleStoreNotActive.Clickable = true;

            _googleStoreActive = FindViewById<ScrollView>(Resource.Id.googleStoreActive);

            _taskHeader = FindViewById<TextView>(Resource.Id.taskHeader);
            _taskHeader.SetTypeface(Rep.FontManager.Get(Font.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);
            _taskDescription = FindViewById<TextView>(Resource.Id.taskDescription);
            _taskDescription.SetTypeface(Rep.FontManager.Get(Font.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);

            _bonusHeader = FindViewById<TextView>(Resource.Id.bonusHeader);
            _bonusHeader.SetTypeface(Rep.FontManager.Get(Font.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);
            _bonusDescription = FindViewById<TextView>(Resource.Id.bonusDescription);
            _bonusDescription.SetTypeface(Rep.FontManager.Get(Font.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);

            _taskListView = FindViewById<ListView>(Resource.Id.taskListView);
            _bonusListView = FindViewById<ListView>(Resource.Id.bonusListView);
        }

        private void GoogleStoreNotActiveOnClick(object sender, EventArgs eventArgs)
        {
            _googleStoreNotActive.Text = string.Empty;
            _googleStoreNotActive.Visibility = ViewStates.Gone;
            _googleStoreActive.Visibility = ViewStates.Visible;
        }

        protected override void RegisterSubscribes()
        {
            _googleStoreNotActive.Click += GoogleStoreNotActiveOnClick;
        }

        protected override void UnregisterSubscribes()
        {
            _googleStoreNotActive.Click -= GoogleStoreNotActiveOnClick;

            if (BonusListAdapter != null) BonusListAdapter.ItemClick -= AdapterOnItemClick;
            if (TaskListAdapter != null) TaskListAdapter.ItemClick -= AdapterOnItemClick;

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
                    ErrorOccur("Нет подключения");
                    return; 
                }
                _billingHandler.OnProductPurchased += BillingHandlerOnProductPurchased;
                _billingHandler.BuyProductError += BillingHandler_BuyProductError;
                Rep.DatabaseHelper.DatabaseChanged += InstanceOnDatabaseChanged;
                await GetInventory();
                if (_allErshProducts == null|| !_allErshProducts.Any()|| _allProducts == null || !_allProducts.Any())
                {
                    ErrorOccur("Нет подключения");
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
                    message = "На данный момент сервис недоступен";
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
                    //    message = "Не удалось купить " + product.Description + "." + System.Environment.NewLine + "Попробуйте позже";
                    //    _billingHandler.ConsumePurchase(purchase);
                    //}
                    break;
            }
            if (string.IsNullOrEmpty(message)) message = "Произошло что-то непредвиденное, повторите свою попытку позже";
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
                var taskCount = Rep.DatabaseHelper.Tasks.Count;
                var allTaskCount = IntConst.AllTaskCount - Rep.DatabaseHelper.Tasks.Count;
                _taskDescription.Text = _purchases.Any(x => x.IsAll && x.ProductType == StringConst.Task)
                    ? string.Format("Вам доступны все {0} заданий и безлимитная подписка на новые",
                        IntConst.AllTaskCount)
                    : string.Format("У вас {0} заданий, {1}", taskCount,
                        allTaskCount == 0 ? "вы можете купить безлимитную подписку на новые" : "можно купить еще " + allTaskCount);

                var bonusCount = Rep.DatabaseHelper.Bonuses.Count;
                var allBonusCount = IntConst.AllBonusCount - Rep.DatabaseHelper.Bonuses.Count;
                _bonusDescription.Text = _purchases.Any(x => x.IsAll && x.ProductType == StringConst.Bonus)
                    ? string.Format("Вам доступны все {0} бонусов и безлимитная подписка на новые",
                        IntConst.AllBonusCount)
                    : string.Format("У вас {0} бонусов, {1}", bonusCount,
                    allBonusCount == 0 ? "вы можете купить безлимитную подписку на новые" : "можно купить еще " + allBonusCount);
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

        private StoreListAdapter TaskListAdapter
        {
            get { return _taskListAdapter; }
            set
            {
                if (_taskListAdapter != null) _taskListAdapter.ItemClick -= AdapterOnItemClick;
                _taskListAdapter = value;
				_taskListAdapter.ItemClick += AdapterOnItemClick;
            }
        }
        private StoreListAdapter BonusListAdapter
        {
            get { return _bonusListAdapter; }
            set
            {
                if (_bonusListAdapter != null) _bonusListAdapter.ItemClick -= AdapterOnItemClick;
                _bonusListAdapter = value;
                _bonusListAdapter.ItemClick += AdapterOnItemClick;
            }
        }
        private void SetupInventory()
        {
            try
            {
                RefreshPurchasedItems();
                SetDescriptionText();
                SetUpProducts();

                TaskListAdapter = new StoreListAdapter(this, _allErshProducts.Where(product => product.Type == StringConst.Task));
                _taskListView.Adapter = new MultiItemRowListAdapter(this, TaskListAdapter, 3, 1);
                _taskListView.JustifyListViewHeightBasedOnChildren();

                BonusListAdapter = new StoreListAdapter(this, _allErshProducts.Where(product => product.Type == StringConst.Bonus));
                _bonusListView.Adapter = new MultiItemRowListAdapter(this, BonusListAdapter, 3, 1);
                _bonusListView.JustifyListViewHeightBasedOnChildren();
                
                _googleStoreActive.Visibility = ViewStates.Visible;
                _googleStoreNotActive.Visibility = ViewStates.Gone;
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
            _googleStoreActive.Visibility = ViewStates.Gone;
            _googleStoreNotActive.Visibility = ViewStates.Visible;
            _googleStoreNotActive.Text = message;
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