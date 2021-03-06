using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Yorsh.Data;
using Yorsh.Helpers;
using Android.Graphics.Drawables;
using Android.Graphics;

namespace Yorsh.Activities
{
    public abstract class BaseActivity : HelpActivity, IBaseActivity
    {
        View _actionButton = null;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                var viewGroup = this.LayoutInflater.Inflate(Resource.Layout.YorshActionBar, null);
                var param = new ActionBar.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.MatchParent);
                var title = viewGroup.FindViewById<TextView>(Resource.Id.titleText);
                title.Text = Title;
                AllowBackPressed = true;
                title.SetTypeface(Rep.FontManager.Get(Font.Bold), TypefaceStyle.Normal);
                ActionBar.SetIcon(new ColorDrawable(Resources.GetColor(Android.Resource.Color.Transparent)));
                ActionBar.SetDisplayShowTitleEnabled(false);
                ActionBar.SetDisplayShowCustomEnabled(true);
                ActionBar.SetCustomView(viewGroup, param);

                SetHomeButtonEnabled(true);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "SetHomeButtonEnabled", exception, false);
            }
        }

        public void SetHomeButtonEnabled(bool enabled)
        {
            try
            {
                ActionBar.SetDisplayHomeAsUpEnabled(enabled);
                ActionBar.SetDisplayShowHomeEnabled(enabled);
                var title = ActionBar.CustomView.FindViewById<TextView>(Resource.Id.titleText);
                title.SetPadding(0, 0, enabled ? 100 : 0, 0);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "SetHomeButtonEnabled", exception, false);
            }

        }

        public View CreateActionButton(int resourceId)
        {

            _actionButton = LayoutInflater.Inflate(resourceId, null);
            try
            {
                var layout = ActionBar.CustomView.FindViewById<RelativeLayout>(Resource.Id.customActionButton);
                var textView = _actionButton as TextView;
                if (textView != null) textView.SetTypeface(Rep.FontManager.Get(Font.Condensed), TypefaceStyle.Normal);
                var param = new RelativeLayout.LayoutParams(
                    ViewGroup.LayoutParams.WrapContent,
                    ViewGroup.LayoutParams.MatchParent);
                param.AddRule(LayoutRules.CenterVertical);
                param.AddRule(LayoutRules.AlignParentRight);
                layout.AddView(_actionButton, param);
            }
            catch (Exception exception)
            {
                GaService.TrackAppException(this.Class, "CreateActionButton", exception, false);
            }

            return _actionButton;
        }

        public View ActionButton
        {
            get { return _actionButton; }
        }
        public override bool OnMenuItemSelected(int featureId, IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                OnPreBackPressed();
                if (AllowBackPressed) this.StartActivityWithoutBackStack(this.ParentActivityIntent);
                return false;
            }
            return base.OnMenuItemSelected(featureId, item);
        }

        public override void OnBackPressed()
        {
            OnPreBackPressed();
            if (AllowBackPressed) this.StartActivityWithoutBackStack(this.ParentActivityIntent);
        }

        public bool AllowBackPressed { get; set; }



        public virtual void OnPreBackPressed()
        {

        }
    }

}


