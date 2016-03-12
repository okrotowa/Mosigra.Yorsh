using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Yorsh.Helpers;

namespace Yorsh.Activities
{
    public abstract class HelpActivity : Activity, View.IOnTouchListener
    {
        private Color _whiteColor;
        private Color _whiteColorWithOpacity;
        private ColorFilter _grayColorFilter;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _whiteColor = Resources.GetColor(Android.Resource.Color.White);
            _whiteColorWithOpacity = this.GetColorWithOpacity(Resource.Color.white, Resource.Color.button_shadow_gray);
            var grayColor = Resources.GetColor(Resource.Color.button_shadow_gray);
            _grayColorFilter = new LightingColorFilter(grayColor, grayColor);
        }

        protected abstract void RegisterSubscribes();
        protected abstract void UnregisterSubscribes();
        protected void AddButtonTouchListener(Button button)
        {
            button.SetOnTouchListener(this);
        }

        public bool OnTouch(View view, MotionEvent e)
        {
            var button = (Button)view;
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    {
                        button.Background.SetColorFilter(_grayColorFilter);
                        button.SetTextColor(_whiteColorWithOpacity);
                        button.Invalidate();
                        return false;
                    }
                case MotionEventActions.Up:
                    {
                        button.Background.ClearColorFilter();
                        button.SetTextColor(_whiteColor);
                        button.Invalidate();
                        return false;
                    }
            }
            return true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterSubscribes();
        }
    }
}