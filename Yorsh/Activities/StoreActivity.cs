using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Content.PM;
using Xamarin.InAppBilling;
using Android.Widget;
using System;
using Android.Views;
using Xamarin.InAppBilling.Model;
using Xamarin.InAppBilling.Utilities;
using Yorsh.Adapters;
using Yorsh.Helpers;
using System.Collections.Generic;
using Android.Content;

namespace Yorsh.Activities
{
<<<<<<< HEAD
    [Activity(Label = "@string/BuyString", ParentActivity = typeof(MainMenuActivity), MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait)]
=======
    [Activity(Label = "@string/BuyString", ParentActivity = typeof (MainMenuActivity), MainLauncher = false,
        ScreenOrientation = ScreenOrientation.Portrait)]
>>>>>>> dbb1a1b64d74397741186207add3383366496c36
    public class StoreActivity : BaseActivity
    {
        private IInAppBillingHelper _billingHelper;
        private IList<Product> _taskProducts;
        private IList<Product> _bonusProducts;
        private InAppBillingServiceConnection _serviceConnection;
        private IList<Purchase> _purchases;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Store);
            FindViewById<TextView>(Resource.Id.taskHeader)
                .SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);
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

            FindViewById<TextView>(Resource.Id.moreHeader)
                .SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);
            var moreDescription = FindViewById<TextView>(Resource.Id.moreDescription);
            moreDescription.SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed),
                Android.Graphics.TypefaceStyle.Bold);

            var morePriceText = FindViewById<TextView>(Resource.Id.morePriceText);
            morePriceText.SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed),
                Android.Graphics.TypefaceStyle.Normal);

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
            if (_serviceConnection != null)
            {
                _serviceConnection.Disconnected();
            }
            base.OnDestroy();
        }

        private void InitPurchasedItems()
        {
            _purchases = _billingHelper.GetPurchases(ItemType.InApp);

        }

        private void OnBuyButtonClick(object sender, EventArgs e)
        {
            //_billingHelper.LaunchPurchaseFlow ("android.test.purchased", ItemType.InApp, Guid.NewGuid().ToString());
            //_billingHelper.LaunchPurchaseFlow(_selectedProduct);
        }

        public void StartSetup()
        {
            var key = string.Concat(new string[]
            {
                GetNumberString(2), GetNumberString(5), GetNumberString(0), GetNumberString(3), GetNumberString(6),
                GetNumberString(7), GetNumberString(1), GetNumberString(4)
            });

            _serviceConnection = new InAppBillingServiceConnection(this, key);
            _serviceConnection.OnConnected += HandleOnConnected;

            _serviceConnection.Connect();
        }

        private async void HandleOnConnected(object sender, EventArgs e)
        {
            _billingHelper = _serviceConnection.BillingHelper;
            InitPurchasedItems();
            await GetInventory();
            SetupInventory();
        }

        private void SetupInventory()
        {
            FindViewById<ScrollView>(Resource.Id.googleStoreActive).Visibility = ViewStates.Visible;
            FindViewById<TextView>(Resource.Id.googleStoreNotActive).Visibility = ViewStates.Gone;
            var taskListView = FindViewById<ListView>(Resource.Id.taskListView);
            var adapter = new StoreListAdapter(this, _taskProducts,
                product => String.CompareOrdinal(product.ProductId, "all_task") == 0);
            taskListView.Adapter = new MultiItemRowListAdapter(this, adapter, 3, 1);
            taskListView.JustifyListViewHeightBasedOnChildren();
            var bonusListView = FindViewById<ListView>(Resource.Id.bonusListView);

            var bonusAdapter = new StoreListAdapter(this, _bonusProducts,
                product => String.CompareOrdinal(product.ProductId, "10_bonus") == 0);
            bonusListView.Adapter = new MultiItemRowListAdapter(this, bonusAdapter, 3, 1);
            bonusListView.JustifyListViewHeightBasedOnChildren();
        }

        private async Task GetInventory()
        {
            //Get available products
            _taskProducts = await _billingHelper.QueryInventoryAsync(new List<string>
            {
                "10_task",
                "30_task",
                "70_task",
                "100_task",
                "all_task"
            }, ItemType.InApp);

            _bonusProducts = await _billingHelper.QueryInventoryAsync(new List<string>()
            {
                "10_bonus",
                "30_bonus",
                "all_bonus"
            }, ItemType.InApp);

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            _billingHelper.HandleActivityResult(requestCode, resultCode, data);

            //TODO: Use a call back to update the purchased items
            // UpdatePurchasedItems();
        }

        public void OnItemSelected(AdapterView parent, Android.Views.View view, int position, long id)
        {
            //_selectedProduct = _taskProducts[position];
        }

        public void OnNothingSelected(AdapterView parent)
        {
            //throw new NotImplementedException ();
        }


        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            //string productid = ((TextView)view).Text;
            //var purchases = _storeListAdapter.Items;
            //var purchasedItem = purchases.FirstOrDefault(p => p.ProductId == productid);
            //if (purchasedItem != null)
            //{
            //    bool result = _billingHelper.ConsumePurchase(purchasedItem);
            //    if (result)
            //    {
            //        _storeListAdapter.Items.Remove(purchasedItem);
            //        _storeListAdapter.NotifyDataSetChanged();
            //    }
            //}
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