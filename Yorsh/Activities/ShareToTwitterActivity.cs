
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Twitter4j.Conf;
using Twitter4j;
using System.Threading;

namespace Yorsh.Activities
{
	[Activity]			
	public class ShareToTwitterActivity : Activity
	{
		ConfigurationBuilder config;
		TwitterFactory factory;
		ITwitter twitter;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Autorize);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button>(Resource.Id.sumbit);

			button.Click += delegate {
				EditText textLogin = FindViewById<EditText> (Resource.Id.login);
				EditText textPass = FindViewById<EditText> (Resource.Id.password);
				string login = textLogin.Text.ToString ();
				string pass = textPass.Text.ToString ();
				// TODO change OAuth keys and tokens and username and password 

				config = new ConfigurationBuilder ();
				config.SetOAuthConsumerKey ("nz2FD8IELdrglLbNMJ1WeMsze");
				config.SetOAuthConsumerSecret ("Q5ZGt7Bc2McY3xdfjks3AU4yw5DKKSv0h3oXCTpjYhV25qwKL0");
				config.SetOAuthAccessToken ("4359603449-6z2CXsqmH4REQayCNgqwq71wM49PjbkSmNIUJil");
				config.SetOAuthAccessTokenSecret ("nAvebHjYLn1JXnO4PwqtmTBhPoet5rhIRUnlKghtmT8Ns");
				config.SetUser (login);
				config.SetPassword (pass);

				factory = new TwitterFactory (config.Build ());
				twitter = factory.Instance;

				GetInfo ();
			};
		}

		void GetInfo(){
			SetContentView (Resource.Layout.EditText);
			Button btn = FindViewById<Button> (Resource.Id.SumbitShare);
			btn.Click += delegate
			{
				var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
				var filePath = System.IO.Path.Combine(sdCardPath, "test.png");

				ThreadPool.QueueUserWorkItem(state =>
					{
						try
						{
							EditText context = FindViewById<EditText>(Resource.Id.TextShare);
							string textShare = context.Text;
							var  f =  new Java.IO.File(filePath);
							StatusUpdate updStatus = new StatusUpdate(textShare);
							updStatus.SetMedia(f);
							twitter.UpdateStatus(updStatus);

						}
						catch (Java.Lang.Exception ex)
						{
							Console.WriteLine(ex);
						}
					});
			};
		}
	}
}

