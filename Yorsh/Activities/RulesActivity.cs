using Android.OS;
using Android.Support.V4.View;
using Android.App;
using Android.Content.PM;
using Android.Widget;

namespace Yorsh.Activities
{
	[Activity(Label = "@string/RulesGameString",ParentActivity = typeof(MainMenuActivity), MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait)]
	public class RulesActivity : BaseActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Rules);

			var adapter = new Adapters.GenericFragmentPagerAdapter(SupportFragmentManager,
					new RuleFragment(Rules.ShortAboutGame)
					,new RuleFragment(Rules.HowToPlay)
					,new RuleFragment(Rules.Bear));
			var viewPager = FindViewById<ViewPager>(Resource.Id.viewPager);
			viewPager.Adapter = adapter;
			viewPager.PageScrollStateChanged += delegate 
			{
				var rulesCirle = Resources.GetStringArray (Resource.Array.RulesCirle);
				var imageScrollId = Resources.GetIdentifier ("rules_" + rulesCirle [viewPager.CurrentItem] + "_sroll_page", "drawable", this.PackageName);
				var imageScroll = FindViewById<ImageView> (Resource.Id.imageScroll);
				imageScroll.SetImageDrawable (Resources.GetDrawable (imageScrollId));
			};
		}
	}
}