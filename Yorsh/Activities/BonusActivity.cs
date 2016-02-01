using Android.App;
using Android.OS;
using Android.Graphics;
using Android.Widget;
using System;
using Yorsh.Model;
using System.Linq;
using Yorsh.Helpers;

namespace Yorsh.Activities
{
	[Activity (Theme = "@android:style/Theme.NoTitleBar", ParentActivity = typeof(GameActivity))]			
	public class BonusActivity : Activity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView(Resource.Layout.Bonus);
			FindViewById<RelativeLayout>(Resource.Id.bonusContentLayout).Background.SetColorFilter (this.Resources.GetColor (Resource.Color.bonus_bg),PorterDuff.Mode.SrcAtop);
			FindViewById<TextView> (Resource.Id.congrats).SetTypeface (this.MyriadProFont (MyriadPro.SemiboldCondensed), TypefaceStyle.Normal);
			FindViewById<TextView> (Resource.Id.youHaveBonus).SetTypeface (this.MyriadProFont (MyriadPro.SemiboldCondensed), TypefaceStyle.Normal);	
			FindViewById<TextView> (Resource.Id.bonusText).SetTypeface (this.MyriadProFont (MyriadPro.Condensed), TypefaceStyle.Normal);
			FindViewById<ImageButton> (Resource.Id.continueButton).Click += (sender, e) => Finish();		
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			var count = new Random ().Next (Rep.DatabaseHelper.Bonuses.Count () - 1);
            FindViewById<TextView>(Resource.Id.bonusText).Text = Rep.DatabaseHelper.Bonuses[count].BonusName;
		}
	}
}

