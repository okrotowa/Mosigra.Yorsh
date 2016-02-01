using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Database.Sqlite;
using Yorsh.Data;
using Yorsh.Model;
using SQLite;
using Stream = System.IO.Stream;
using Android.Graphics;
using Android.Widget;
using Android.Views;

namespace Yorsh.Helpers
{
    public static class ActivityExtensions
    {
        private static readonly object Locker = new object();

        public static void StartActivityWithoutBackStack(this Activity mainActivity, Intent newActivity)
        {
            mainActivity.StartActivity(newActivity);
            mainActivity.Finish();
        }

        public static Typeface MyriadProFont(this Activity activity, MyriadPro myriadPro)
        {
            string name;
            switch (myriadPro)
            {
                case MyriadPro.BoldCondensed:
                    name = "MyriadProBoldCondensed.ttf";
                    break;
                case MyriadPro.Condensed:
                    name = "MyriadProCondensed.ttf";
                    break;
                case MyriadPro.Bold:
                    name = "MyriadProBold.ttf";
                    break;
                case MyriadPro.Regular:
                    name = "MyriadProRegular.ttf";
                    break;
                case MyriadPro.SemiboldCondensed:
                    name = "MyriadProSemiboldCond.otf";
                    break;
                case MyriadPro.LightCondensed:
                    name = "MyriadProLightCond.otf";
                    break;
                default:
                    throw new NotImplementedException();
            }
            return Typeface.CreateFromAsset(activity.Assets, name);
        }

        public static Typeface BankirRetroFont(this Activity activity)
        {
            return Typeface.CreateFromAsset(activity.Assets, "BankirRetro.ttf");
        }

        //public static async Task CreateOrOpenDataBaseAsync(this Activity context)
        //{
        //    try
        //    {
        //        var connection = Rep.Instance.DataBaseConnection;
        //        var results = await connection.CreateTablesAsync<TaskTable, BonusTable, CategoryTable>();

        //        if (results.Results.Count == 3)
        //        {
        //            var tasks = GetTasks(context.Assets.Open("Task.csv"), 0, IntConst.DefaultTaskCount);
        //            var bonuses = GetBonus(context.Assets.Open("Bonus.csv"), 0, IntConst.DefaultBonusCount);
        //            var category = GetCategory(context.Assets.Open("Category.csv"));

        //            await connection.InsertAllAsync(tasks);
        //            await connection.InsertAllAsync(bonuses);
        //            await connection.InsertAllAsync(category);
        //        }

        //    }
        //    catch (Exception exception)
        //    {
        //        GaService.TrackAppException(context.Class.SimpleName, "CreateOrOpenDataBaseAsync", exception, true);
        //    }

        //}

        

        //public static async Task AddProductAsync(this Activity context, string countProduct, string name)
        //{
        //    try
        //    {
        //        if (string.CompareOrdinal("task", name) == 0)
        //        {
        //            var count = 0;
        //            if (!int.TryParse(countProduct, out count) && string.CompareOrdinal(countProduct, "all") == 0)
        //                count = IntConst.AllTaskCount - Rep.Instance.Tasks.Count();
        //            if (count == 0)
        //                return;
        //            await AddTask(context.Assets.Open("Task.csv"), count);
        //        }
        //        else if (string.CompareOrdinal("bonus", name) == 0)
        //        {
        //            int count = 0;
        //            if (!int.TryParse(countProduct, out count) && string.CompareOrdinal(countProduct, "all") == 0)
        //                count = IntConst.AllBonusCount - Rep.Instance.Bonuses.Count();
        //            if (count == 0)
        //                return;
        //            await AddBonus(context.Assets.Open("Bonus.csv"), count);
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        GaService.TrackAppException(context.Class.SimpleName, "AddProductAsync", exception, true);
        //    }

        //}
        //public static async Task AddProductAsync(this Activity context, string skuId)
        //{
        //    try
        //    {
        //        var skuItems = skuId.Split('_');
        //        if (skuItems.Count() != 2) return;

        //        if (string.CompareOrdinal("task", skuItems[1]) == 0)
        //        {
        //            var count = 0;
        //            if (!int.TryParse(skuItems[0], out count) && string.CompareOrdinal(skuItems[0], "all") == 0)
        //                count = IntConst.AllTaskCount - Rep.Instance.Tasks.Count();
        //            if (count == 0)
        //                return;
        //            await AddTask(context.Assets.Open("Task.csv"), count);
        //        }
        //        else if (string.CompareOrdinal("bonus", skuItems[1]) == 0)
        //        {
        //            int count = 0;
        //            if (!int.TryParse(skuItems[0], out count) && string.CompareOrdinal(skuItems[0], "all") == 0)
        //                count = IntConst.AllBonusCount - Rep.Instance.Bonuses.Count();
        //            if (count == 0)
        //                return;
        //            await AddBonus(context.Assets.Open("Bonus.csv"), count);
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        GaService.TrackAppException(context.Class.SimpleName, "AddProductAsync", exception, true);
        //    }

        //}



        public static void OnTouchButtonDarker(this Activity activity, Button sender, View.TouchEventArgs e)
        {
            var button = sender;
            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    {
                        button.Background.SetColorFilter(activity.Resources.GetColor(Resource.Color.button_shadow_gray), PorterDuff.Mode.SrcAtop);
                        button.SetTextColor(GetColorWithOpacity(activity, Resource.Color.white, Resource.Color.button_shadow_gray));
                        button.Invalidate();
                        e.Handled = false;
                        break;
                    }
                case MotionEventActions.Up:
                    {
                        button.Background.ClearColorFilter();
                        button.SetTextColor(activity.Resources.GetColor(Android.Resource.Color.White));
                        button.Invalidate();
                        e.Handled = false;
                        break;
                    }
            }
        }
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
                GaService.TrackAppException(activity.Class.SimpleName, "SaveAsStartupActivity", ex, false);
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


public enum MyriadPro
{
    BoldCondensed,
    Condensed,
    Bold,
    Regular,
    SemiboldCondensed,
    LightCondensed
}