using Android.App;
using Android.OS;
using Android.Content.PM;
using Xamarin.InAppBilling;
using Android.Widget;

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

            //TODO::Delete
			//var m = @"MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAr1wsRhQz/aFBC2HGRbIJ0Dx372QXWoSYBZT8nESCUl9dxhILH+dSS2UN43ovQsqAfb1WNQ+qvc8aVwNq0BXiVmNecqLYbsL2jZSGS7ox/Y8Xl03wsQfVHoOY6LDORvYN+ZqJxEuS1dqcUul4TiUqbemZKOAqNQeRUmMz/QmCnNiINuaOv3vOQKMyJsYPwqg+9oei/9bYSIPdkXiyHATbEIiMULheQrS0w4fMu8Fk6nqAMmp8RAXX/fCDCIoIkJ42kIpiUY9k8qeqHeZkrTVMi//AbuWK7Hx+4hhf6gIR4U4Mz544aXX93w56OTgnC32Ngqoa8RkzHpQPoS5dvLUMMwIDAQAB";
			 
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
            public override StoreItem this[int position]
            {
                get { throw new System.NotImplementedException(); }
            }

            
            public override int Count
            {
                get { throw new System.NotImplementedException(); }
            }

            public override long GetItemId(int position)
            {
                throw new System.NotImplementedException();
            }
			public override Android.Views.View GetView (int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
			{
				throw new System.NotImplementedException ();
			}
        }

        private class StoreItem
        {
            public StoreItem()
            { 
                
            }
        }
    }
}