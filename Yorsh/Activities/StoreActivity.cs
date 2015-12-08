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
	[Activity(Label = "@string/BuyString", ParentActivity = typeof(MainMenuActivity), MainLauncher=false,ScreenOrientation = ScreenOrientation.Portrait)]
    public class StoreActivity : BaseActivity
    {
		private InAppBillingServiceConnection _connection;

		protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Store);
			var key = Xamarin.InAppBilling.Utilities.Security.Unify (
				new string[] {	GetNumberString(2),GetNumberString(5),GetNumberString(0),GetNumberString(3),GetNumberString(6),GetNumberString(7),GetNumberString(1),GetNumberString(4)},
				new int[]{ 0, 1, 2, 3, 4, 5, 6, 7 });
			_connection = new InAppBillingServiceConnection (this, key);
            var taskListView = FindViewById<ListView>(Resource.Id.taskListView);
			var adapter = new StoreListAdapter (this, BuyElement.Task,_connection);
			taskListView.Adapter = new MultiItemRowListAdapter(this, adapter,3,1);
			taskListView.JustifyListViewHeightBasedOnChildren ();
			var bonusListView = FindViewById<ListView> (Resource.Id.bonusListView);
			bonusListView.Adapter = new MultiItemRowListAdapter (this, new StoreListAdapter (this, BuyElement.Bonus, _connection), 3, 1);
			bonusListView.JustifyListViewHeightBasedOnChildren ();			
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
		protected override void OnDestroy ()
		{
			if (_connection != null)
				_connection.Disconnect ();
			base.OnDestroy ();
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			// Ask the open service connection's billing handler to process this request
			_connection.BillingHandler.HandleActivityResult (requestCode, resultCode, data);
			var purchases = _connection.BillingHandler.GetPurchases (ItemType.Product);	
			// TODO: Use a call back to update the purchased items
			// or listen to the OnProductPurchased event to
			// handle a successful purchase
		}

		private sealed class StoreListAdapter : BaseAdapter<StoreItem>
        {
			StoreItem[] _storeItems;
			Activity _context;
			IList<Product> _products;
			private InAppBillingServiceConnection _connection;

			public StoreListAdapter(Activity context, BuyElement buyElement, InAppBillingServiceConnection connection)
			{
				_context = context;
				_connection = connection;
				switch(buyElement)
				{
					case BuyElement.Task: 
						_storeItems = new StoreItem[5];
						_storeItems[0] = new StoreItem(29,"10",buyElement);
						_storeItems[1] = new StoreItem(59,"30",buyElement);
						_storeItems[2] = new StoreItem(119,"70",buyElement);
						_storeItems[3] = new StoreItem(169,"100",buyElement);
					_storeItems[4] = new StoreItem(279,"∞",buyElement,"task_sale");
					break;
					case BuyElement.Bonus: 
						_storeItems = new StoreItem[3];
						_storeItems[0] = new StoreItem(15,"10",buyElement,"bonus_sale");
						_storeItems[1] = new StoreItem(59,"30",buyElement);
						_storeItems[2] = new StoreItem(129,"∞",buyElement);
					break;
				}
				_connection.OnConnected += async () => 				
											_products = await _connection.BillingHandler.QueryInventoryAsync (new List<string> {
											ReservedTestProductIDs.Purchased,
											ReservedTestProductIDs.Canceled,
											ReservedTestProductIDs.Refunded,
											ReservedTestProductIDs.Unavailable
										}, ItemType.Product);
				
							
				_connection.Connect();
			}
		
            public override StoreItem this[int position]
            {
				get { return _storeItems[position]; }
            }

            
            public override int Count
            {
				get { return _storeItems.Count(); }
            }

            public override long GetItemId(int position)
            {
				return position;
            }

			public override View GetView (int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
			{
				if (convertView != null) return convertView;
				var storeItem = this [position];
				var view = _context.LayoutInflater.Inflate (Resource.Layout.StoreItem, null);		
				var button = view.FindViewById<RelativeLayout> (Resource.Id.storeButton);
				button.SetBackgroundResource (storeItem.BuyElement == BuyElement.Bonus 
					? Resource.Drawable.bonus_store_icon
					: Resource.Drawable.task_store_icon
				);
				button.Click += (sender, e) => 
				{
					
				};
				if (!string.IsNullOrEmpty (storeItem.SaleImageString)) 
					view.FindViewById<ImageView>(Resource.Id.saleImageView).SetImageResource(
						_context.Resources.GetIdentifier(storeItem.SaleImageString, "drawable", _context.PackageName));
				var countText = view.FindViewById<TextView> (Resource.Id.countText);
				countText.Text = storeItem.Count;
				var priceText = view.FindViewById<TextView> (Resource.Id.priceText);
				priceText.Text = storeItem.Price + " руб.";
				view.FindViewById<TextView> (Resource.Id.unitText).Visibility = position == this.Count - 1 
					? ViewStates.Gone
					: ViewStates.Visible;
				
				return view;
			}
        }

		private sealed class StoreItem
        {
			public StoreItem(float price, string count, BuyElement buyElement, string saleImageString = null)
            { 
				Price = price;
				Count = count;
				BuyElement = buyElement;
				SaleImageString=saleImageString;
				IsBuy = false;
            }

			public float Price { get; private set;}
			public string Count { get; private set;}
			public BuyElement BuyElement { get; private set;}
			public string SaleImageString { get; private set;}


			public bool IsBuy { get; set;} 
        }
   
				private enum BuyElement
				{
					Task,
					Bonus
				}
	}
}