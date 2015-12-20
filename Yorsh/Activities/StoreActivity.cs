using System.IO;
using System.Runtime.Remoting.Messaging;
using Android.App;
using Android.OS;
using Android.Content.PM;
using Xamarin.InAppBilling;
using Android.Widget;
using System;
using System.Linq;
using Android.Views;
using Yorsh.Adapters;
using Yorsh.Helpers;
using System.Collections.Generic;
using Android.Content;

namespace Yorsh.Activities
{
    [Activity(Label = "@string/BuyString", ParentActivity = typeof(MainMenuActivity), MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait)]
    public class StoreActivity : BaseActivity
    {
        private InAppBillingServiceConnection _connection;
        private const string Task = "task";
        private const string Bonus = "bonus";
        private IList<Product> product;
        private IList<Product> _products
        {
            get
            {
                return product;
            }

            set
            {
                product = value;
            }
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Store);
            FindViewById<TextView>(Resource.Id.taskHeader).SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);
            var taskDescription = FindViewById<TextView>(Resource.Id.taskDescription);
            taskDescription.SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);

            FindViewById<TextView>(Resource.Id.googleStoreNotActive).SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);

            FindViewById<TextView>(Resource.Id.bonusHeader).SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);
            var bonusDescription = FindViewById<TextView>(Resource.Id.bonusDescription);
            bonusDescription.SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);

            FindViewById<TextView>(Resource.Id.moreHeader).SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);
            var moreDescription = FindViewById<TextView>(Resource.Id.moreDescription);
            moreDescription.SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Bold);

            var morePriceText = FindViewById<TextView>(Resource.Id.morePriceText);
            morePriceText.SetTypeface(this.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Normal);

            var key = Xamarin.InAppBilling.Utilities.Security.Unify(
                new string[] { GetNumberString(2), GetNumberString(5), GetNumberString(0), GetNumberString(3), GetNumberString(6), GetNumberString(7), GetNumberString(1), GetNumberString(4) },
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });

            _connection = new InAppBillingServiceConnection(this, key);
            _connection.OnInAppBillingError += _connection_OnInAppBillingError;
            _connection.BillingHandler.InAppBillingProcesingError += ErrorOccur;
            _connection.OnConnected += ConnectionOnOnConnected;
            _connection.Connect();
        }

        private async void ConnectionOnOnConnected()
        {
            try
            {
                _products = await _connection.BillingHandler.QueryInventoryAsync(new List<string>
                {
                    ReservedTestProductIDs.Purchased,
                    ReservedTestProductIDs.Canceled,
                    ReservedTestProductIDs.Refunded,
                    ReservedTestProductIDs.Unavailable
                }, ItemType.Product);

                if (_products != null)
                {
                    FindViewById<ScrollView>(Resource.Id.googleStoreActive).Visibility = ViewStates.Visible;
                    FindViewById<TextView>(Resource.Id.googleStoreNotActive).Visibility = ViewStates.Gone;
                    var taskListView = FindViewById<ListView>(Resource.Id.taskListView);
                    var adapter = new StoreListAdapter(this, _products.Where(taskProduct => taskProduct.ProductId.Contains("tasks")));
                    taskListView.Adapter = new MultiItemRowListAdapter(this, adapter, 3, 1);
                    taskListView.JustifyListViewHeightBasedOnChildren();
                    var bonusListView = FindViewById<ListView>(Resource.Id.bonusListView);
                    var bonusAdapter = new StoreListAdapter(this, _products.Where(bonusProduct => bonusProduct.ProductId.Contains("bonuses")));
                    bonusListView.Adapter = new MultiItemRowListAdapter(this, bonusAdapter, 3, 1);
                    bonusListView.JustifyListViewHeightBasedOnChildren();
                }
                else
                {
                    throw new InvalidDataException("Google Store работает не правильно");
                }
            }

            catch (Exception ex)
            {
                ErrorOccur("Произошло что-то непонятное/n" + ex.Message);
            }
        }

        void ErrorOccur(string message)
        {
            FindViewById<ScrollView>(Resource.Id.googleStoreActive).Visibility = ViewStates.Gone;
            var googleStoreNotActive = FindViewById<TextView>(Resource.Id.googleStoreNotActive);
            googleStoreNotActive.Visibility = ViewStates.Visible;
            googleStoreNotActive.Text = message;
        }


        private void _connection_OnInAppBillingError(InAppBillingErrorType error, string message)
        {
            switch (error)
            {
                case InAppBillingErrorType.BillingNotSupported:
                    ErrorOccur("Google Store не поддерживается вашим устройством");
                    break;
                default:
                    ErrorOccur("Произошло что-то непонятное");
                    break;
            }
        }

        private string GetNumberString(int num)
        {
            var m = new string[]
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

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            // Ask the open service connection's billing handler to process this request
            _connection.BillingHandler.HandleActivityResult(requestCode, resultCode, data);
            var purchases = _connection.BillingHandler.GetPurchases(ItemType.Product);
            // TODO: Use a call back to update the purchased items
            // or listen to the OnProductPurchased event to
            // handle a successful purchase
        }

        private sealed class StoreListAdapter : BaseAdapter<StoreItem>
        {
            readonly Activity _context;
            readonly List<StoreItem> _products;
            public StoreListAdapter(Activity context, IEnumerable<Product> products)
            {
                _context = context;
                var productList = (from prod in products let isSale = String.CompareOrdinal(prod.ProductId, "all_task") == 0 || String.CompareOrdinal(prod.ProductId, "10_bonus") == 0 select new StoreItem(prod, isSale)).ToList();
                _products = productList.OrderBy(item => item.CountForSort).ToList();
            }

            public override StoreItem this[int position]
            {
                get { return _products[position]; }
            }


            public override int Count
            {
                get { return _products.Count(); }
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                if (convertView != null) return convertView;
                var storeItem = this[position];
                var view = _context.LayoutInflater.Inflate(Resource.Layout.StoreItem, null);
                var button = view.FindViewById<ImageButton>(Resource.Id.storeButton);
                var drawable = _context.Resources.GetDrawable(_context.Resources.GetIdentifier(storeItem.ImageString, "drawable", _context.PackageName));
                button.SetImageDrawable(drawable);

                button.Click += (sender, e) =>
                {
                    //buy something
                };
                var saleImage = view.FindViewById<ImageView>(Resource.Id.saleImageView);
                saleImage.Visibility = storeItem.IsSale ? ViewStates.Visible : ViewStates.Invisible;
                if (storeItem.IsSale) saleImage.SetImageResource(_context.Resources.GetIdentifier(storeItem.SaleImageString, "drawable", _context.PackageName));

                var priceText = view.FindViewById<TextView>(Resource.Id.priceText);
                priceText.SetTypeface(_context.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Normal);
                priceText.Text = storeItem.Product.Price + " " + storeItem.Product.Price_Currency_Code;

                return view;
            }
        }

        private sealed class StoreItem
        {
            public StoreItem(Product product, bool isSale = false)
            {
                Product = product;
                var splitProductId = Product.ProductId.Split('_');
                int count;
                CountForSort = int.TryParse(splitProductId[0], out count) ? count : 1000;
                BuyElement = splitProductId[1];
                IsSale = isSale;
                ImageString = string.Format("shop_{0}_{1}", BuyElement, splitProductId[0]);
                SaleImageString = isSale ? BuyElement + "_sale" : null;
            }

            public Product Product { get; private set; }
            public int CountForSort { get; private set; }
            public string BuyElement { get; private set; }
            public bool IsSale { get; private set; }
            public string ImageString { get; private set; }
            public string SaleImageString { get; private set; }
        }

    }
}