﻿using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using Yorsh.Data;
using Yorsh.Helpers;

namespace Yorsh.Activities
{
	public class RuleFragment : Android.Support.V4.App.Fragment
	{
		private Rules _rule;
		public RuleFragment(Rules rule)
		{
			_rule = rule;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var view = inflater.Inflate (Resource.Layout.RuleFragment, null);
			var imageBackgroundRules= view.FindViewById<ImageView> (Resource.Id.imageBackgroundRules);
			var arr = Resources.GetStringArray (Resources.GetIdentifier(GetRuleName(_rule),"array", Activity.PackageName));
			var imageId = Resources.GetIdentifier ("rules_" + GetRuleIdentifier(_rule) + "_page", "drawable", Activity.PackageName);
			imageBackgroundRules.SetImageResource(imageId);
			var textHeader = view.FindViewById<TextView> (Resource.Id.textHeader);
			textHeader.Text = arr [0];
            textHeader.SetTypeface(Rep.FontManager.Get(Font.BoldCondensed), Android.Graphics.TypefaceStyle.Normal);
			var textContainer = view.FindViewById<TextView> (Resource.Id.textContainer);
			textContainer.Text = arr [1];
            textContainer.SetTypeface(Rep.FontManager.Get(Font.Condensed), Android.Graphics.TypefaceStyle.Normal);
			return view;
		}
		private string GetRuleIdentifier(Rules rule)
		{
			switch (rule)
			{
				case Rules.ShortAboutGame:
					return "one";
				case Rules.HowToPlay:
					return "two";
				case Rules.Bear:
					return "three";
				default: throw new NotImplementedException();
			}
		}
		private string GetRuleName(Rules rule)
		{
			switch (rule) 
			{
				case Rules.ShortAboutGame:
					return "ShortAboutGame";
				case Rules.HowToPlay:
					return "HowToPlay";
				case Rules.Bear:
					return "AndSuddenly";
				default: throw new NotImplementedException();
			}
		}
	}
}
public enum  Rules
{
	ShortAboutGame,
	HowToPlay,
	Bear
}

