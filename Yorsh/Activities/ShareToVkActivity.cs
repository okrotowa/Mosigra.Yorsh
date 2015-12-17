using Android.App;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using Xamarin.Auth;
using Android.Net;
using System.Xml;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Runtime;
using Android.Views;
using System.Net;
using System.Threading;
using Android.Graphics;
using System.IO;
using Org.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Yorsh.Activities
{
	[Activity]			
	public class ShareToVkActivity : Activity
	{
		public string token;
		public string userId;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			LoginToVk ();
		}

		void LoginToVk ()
		{
			var auth = new OAuth2Authenticator (
				clientId: "5171316",  // id  нашого додатку 
				scope: "wall,photos,notes,pages", // запит прав нашого додатку 
				authorizeUrl: new System.Uri ("https://oauth.vk.com/authorize"),
				redirectUrl: new System.Uri ("https://oauth.vk.com/blank.html"));

			auth.AllowCancel = true;
			auth.Completed += (s, ee) => { //провірка чи аудентифіколваний
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
			var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
			var filePath = System.IO.Path.Combine(sdCardPath, "test.png");
			SetContentView (Resource.Layout.EditText);
			Button btn = FindViewById<Button> (Resource.Id.SumbitShare);
			btn.Click += delegate {
				
				EditText text = FindViewById<EditText>(Resource.Id.TextShare);
				string stext = "Мы сыграли в Ёрша. Победитель спит под столом :)";
		
				var c = new WebClient();
				//
				var u = "https://api.vk.com/method/photos.getWallUploadServer?user_id=" + userId 
					+ "&access_token=" + token;
				var r = c.DownloadString(u);
				var j = JsonConvert.DeserializeObject(r) as JObject;
				//
				var u2 = j["response"]["upload_url"].ToString();
				var r2 = Encoding.UTF8.GetString(c.UploadFile(u2, "POST", filePath));
				var j2 = JsonConvert.DeserializeObject(r2) as JObject;
				//
				var u3 = "https://api.vk.com/method/photos.saveWallPhoto?access_token=" + token
					+ "&server=" + j2["server"]
					+ "&photo=" + j2["photo"]
					+ "&hash=" + j2["hash"];
				var r3 = c.DownloadString(u3);
				var j3 = JsonConvert.DeserializeObject(r3) as JObject;
				// 
				var u4 = "https://api.vk.com/method/wall.post?access_token=" + token
					+ "&owner_id" + j3["response"][0]["owner_id"]
					+ "&message=" + stext
					+ "&attachments=" + j3["response"][0]["id"];
				c.DownloadString(u4);
			};
		}

		private static readonly TaskScheduler UIScheduler = TaskScheduler.FromCurrentSynchronizationContext();
	}
}

