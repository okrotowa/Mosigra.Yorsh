using System;
using Android.OS;
using Android.Support.V4.View;
using Android.App;
using Android.Content.PM;
using Android.Widget;

namespace Yorsh.Activities
{
	[Activity(Label = "@string/RulesGameString",ParentActivity = typeof(MainMenuActivity), MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait)]
	public class RulesActivity : BaseFragmentActivity
	{
	    private ViewPager _viewPager;

	    protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Rules);

			var adapter = new Adapters.GenericFragmentPagerAdapter(SupportFragmentManager,
					new RuleFragment(Rules.ShortAboutGame)
					,new RuleFragment(Rules.HowToPlay)
					,new RuleFragment(Rules.Bear));
			_viewPager = FindViewById<ViewPager>(Resource.Id.viewPager);
            _viewPager.Adapter = adapter;
            _viewPager.PageScrollStateChanged += ViewPagerOnPageScrollStateChanged; 
		}

	    private void ViewPagerOnPageScrollStateChanged(object sender, ViewPager.PageScrollStateChangedEventArgs pageScrollStateChangedEventArgs)
	    {
            var rulesCirle = Resources.GetStringArray(Resource.Array.RulesCirle);
            var imageScrollId = Resources.GetIdentifier("rules_" + rulesCirle[_viewPager.CurrentItem] + "_sroll_page", "drawable", this.PackageName);
            var imageScroll = FindViewById<ImageView>(Resource.Id.imageScroll);
            imageScroll.SetImageResource(imageScrollId);
	    }

	    protected override void OnDestroy()
	    {
	        base.OnDestroy();
            _viewPager.PageScrollStateChanged -= ViewPagerOnPageScrollStateChanged;
	    }
	}
}