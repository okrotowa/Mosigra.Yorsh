using Android.App;
using Android.OS;
using Android.Content.PM;
using Xamarin.InAppBilling;
using Android.Widget;
using System;
using System.Linq;
using Android.Views;
using Yorsh.Adapters;

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
            var taskListView = FindViewById<ListView>(Resource.Id.taskListView);
			taskListView.Adapter = new MultiItemRowListAdapter(this, new StoreListAdapter(this, BuyElement.Task),3,1);
			var bonusListView = FindViewById<ListView> (Resource.Id.bonusListView);
			bonusListView.Adapter = new MultiItemRowListAdapter (this, new StoreListAdapter (this, BuyElement.Bonus), 3, 1);
			var key = Xamarin.InAppBilling.Utilities.Security.Unify (
						 new string[] {	GetNumberString(2),GetNumberString(5),GetNumberString(0),GetNumberString(3),GetNumberString(6),GetNumberString(7),GetNumberString(1),GetNumberString(4)},
				         new int[]{ 0, 1, 2, 3, 4, 5, 6, 7 });
			_connection = new InAppBillingServiceConnection (this, key);
			_connection.OnConnected += () => 
			{

			};
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
        private class StoreListAdapter : BaseAdapter<StoreItem>
        {
			StoreItem[] _storeItems;
			Activity _context;
			public StoreListAdapter(Activity context, BuyElement buyElement)
			{
				_context = context;
				switch(buyElement)
				{
					case BuyElement.Task: 
						_storeItems = new StoreItem[5];
						_storeItems[0] = new StoreItem(29,"10",buyElement);
						_storeItems[1] = new StoreItem(59,"30",buyElement);
						_storeItems[2] = new StoreItem(119,"70",buyElement);
						_storeItems[3] = new StoreItem(169,"100",buyElement);
						_storeItems[4] = new StoreItem(279,"∞",buyElement){SaleImageString = "task_sale"};
					break;
					case BuyElement.Bonus: 
						_storeItems = new StoreItem[3];
						_storeItems[0] = new StoreItem(15,"10",buyElement){SaleImageString = "bonus_sale"};
						_storeItems[1] = new StoreItem(59,"30",buyElement);
						_storeItems[2] = new StoreItem(129,"∞",buyElement);
					break;
				}
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

			public override Android.Views.View GetView (int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
			{
				if (convertView != null) return convertView;
				var storeItem = this [position];
				var view = _context.LayoutInflater.Inflate (Resource.Layout.StoreItem, null);		
				var button = view.FindViewById<RelativeLayout> (Resource.Id.storeButton);
				button.SetBackgroundResource (storeItem.BuyElement == BuyElement.Bonus 
					? Resource.Drawable.bonus_store_icon
					: Resource.Drawable.task_store_icon
				);
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
		


        private class StoreItem
        {
			public StoreItem(float price, string count, BuyElement buyElement, bool isBuy = false)
            { 
				Price = price;
				Count = count;
				BuyElement = buyElement;
				IsBuy = isBuy;
            }

			public float Price { get; set;}
			public string Count { get; set;}
			public bool IsBuy { get; set;}
			public BuyElement BuyElement { get; set;}
			public string SaleImageString { get; set;}
        }
   
				private enum BuyElement
				{
					Task,
					Bonus
				}
	}
}