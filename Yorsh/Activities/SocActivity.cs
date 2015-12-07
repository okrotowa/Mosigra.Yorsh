using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System.Xml;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Auth;

namespace Yorsh.Activities
{
	[Activity (Label = "SocActivity",MainLauncher = false)]			
	public class SocActivity : Activity
	{
			public string token;
			public string userId;

			void LoginToVk ()
			{
				var auth = new OAuth2Authenticator (
					clientId: "5171316",  // id  нашого додатку 
					scope: "friends,wall,photos,notes,pages", // запит прав нашого додатку 
					authorizeUrl: new System.Uri ("https://oauth.vk.com/authorize"),
					redirectUrl: new System.Uri ("https://oauth.vk.com/blank.html"));

				auth.AllowCancel = true;
				auth.Completed += (s, ee) => {
					if (!ee.IsAuthenticated) {
						var builder = new AlertDialog.Builder (this);
						builder.SetMessage ("Not Authenticated");
						builder.SetPositiveButton ("Ok", (o, e) => { });
						builder.Create().Show();
						return;
					}
					else
					{
						token = ee.Account.Properties ["access_token"].ToString ();
						userId = ee.Account.Properties ["user_id"].ToString ();	
						GetInfo();
					}
				};
				var intent = auth.GetUI (this);
				StartActivity (intent);
			}

			void GetInfo()
			{
				SetContentView (Resource.Layout.text);
				Button btn = FindViewById<Button> (Resource.Id.button1);
				btn.Click += delegate {
				EditText text = FindViewById<EditText>(Resource.Id.editText1);
				string stext = text.Text.ToString();
				WebRequest webRequest = WebRequest.Create ("https://api.vk.com/method/wall.post.xml?owner_id="+userId+"&message="+stext+"&access_token="+ token);

				XmlDocument xmlDocument = new XmlDocument ();
				WebResponse webResponse = webRequest.GetResponse ();
				Stream stream = webResponse.GetResponseStream ();
				xmlDocument.Load (stream);
			};
				
			}


			private static readonly TaskScheduler UIScheduler = TaskScheduler.FromCurrentSynchronizationContext();

			protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.ShareSoc);
			var vk = FindViewById<Button> (Resource.Id.buttonVk);			
			vk.Click += delegate {
				LoginToVk ();
			};
		}
	}
}

