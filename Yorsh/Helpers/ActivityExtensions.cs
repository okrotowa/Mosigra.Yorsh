using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using Android.Views;

namespace Yorsh.Helpers
{
    public static class ActivityExtensions
    {

        public static void StartActivityWithoutBackStack(this Activity mainActivity, Intent newActivity)
        {
            mainActivity.StartActivity(newActivity);
            mainActivity.Finish();
        }

        //public static Typeface MyriadProFont(this Activity activity, Font font)
        //{
        //    string name;
        //    switch (font)
        //    {
        //        case Font.BoldCondensed:
        //            name = "MyriadProBoldCondensed.ttf";
        //            break;
        //        case Font.Condensed:
        //            name = "MyriadProCondensed.ttf";
        //            break;
        //        case Font.Bold:
        //            name = "MyriadProBold.ttf";
        //            break;
        //        case Font.Regular:
        //            name = "MyriadProRegular.ttf";
        //            break;
        //        case Font.SemiboldCondensed:
        //            name = "MyriadProSemiboldCond.otf";
        //            break;
        //        case Font.LightCondensed:
        //            name = "MyriadProLightCond.otf";
        //            break;
        //        default:
        //            throw new NotImplementedException();
        //    }
        //    return Typeface.CreateFromAsset(activity.Assets, name);
        //}

        //public static void OnTouchButtonDarker(this Activity activity, Button sender, View.TouchEventArgs e)
        //{
        //    var button = sender;
        //    switch (e.Event.Action)
        //    {
        //        case MotionEventActions.Down:
        //            {
        //                button.Background.SetColorFilter(activity.Resources.GetColor(Resource.Color.button_shadow_gray), PorterDuff.Mode.SrcAtop);
        //                button.SetTextColor(GetColorWithOpacity(activity, Resource.Color.white, Resource.Color.button_shadow_gray));
        //                button.Invalidate();
        //                e.Handled = false;
        //                break;
        //            }
        //        case MotionEventActions.Up:
        //            {
        //                button.Background.ClearColorFilter();
        //                button.SetTextColor(activity.Resources.GetColor(Android.Resource.Color.White));
        //                button.Invalidate();
        //                e.Handled = false;
        //                break;
        //            }
        //    }
        //}
        public static void SaveCurrentPlayer(this Activity activity, int player)
        {
            var editor = activity.GetSharedPreferences("T", FileCreationMode.Private).Edit();
            editor.PutInt("currentPlayer", player);
            editor.Commit();
        }

        public static int GetCurrentPlayer(this Activity activity)
        {
            var prefs = activity.GetSharedPreferences("T", FileCreationMode.Private);
            var count = prefs.GetInt("currentPlayer", 0);
            return count < 0 ? 0 : count;
        }

        public static void SaveAsStartupActivity(this Activity activity, string activityName)
        {
            try
            {
                var editorX = activity.GetSharedPreferences("X", FileCreationMode.Private).Edit();
                editorX.PutString("lastActivity", activityName);
                editorX.Commit();
            }
            catch (Exception ex)
            {
                GaService.TrackAppException(activity.Class, "SaveAsStartupActivity", ex, false);
            }
        }

        public static Color GetColorWithOpacity(this Activity activity, int normalColor, int opacityColor)
        {
            var bitmap = Bitmap.CreateBitmap(1, 1, Bitmap.Config.Argb8888); //make a 1-pixel Bitmap
            var canvas = new Canvas(bitmap);
            canvas.DrawColor(activity.Resources.GetColor(normalColor)); //color we want to apply filter to
            canvas.DrawColor(activity.Resources.GetColor(opacityColor), PorterDuff.Mode.SrcAtop); //apply filter
            var index = bitmap.GetPixel(0, 0);
            return Color.Argb(index, index, index, index);
        }

    }
}


public enum Font
{
    BoldCondensed,
    Condensed,
    Bold,
    Regular,
    SemiboldCondensed,
    LightCondensed,
    BankirRetro
}