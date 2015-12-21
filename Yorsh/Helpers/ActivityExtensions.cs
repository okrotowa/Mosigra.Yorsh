using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Yorsh.Activities;
using Yorsh.Model;
using SQLite;
using File = System.IO.File;
using Stream = System.IO.Stream;
using Android.Graphics;
using Android.Widget;
using Android.Views;

namespace Yorsh.Helpers
{
    public static class ActivityExtensions
    {
        public static void StartActivityWithoutBackStack(this Activity mainActivity, Intent newActivity,
            int fragment = -1)
        {
            if (fragment > 0) newActivity.PutExtra("fragment", fragment);
            mainActivity.StartActivity(newActivity);
            mainActivity.Finish();
        }

        public static async Task StubInitialize(this Activity activity)
        {
            if (Rep.Instance.Players.Count != 0)
                return;
            Rep.Instance.Players.Add("Olga", BitmapFactory.DecodeResource(activity.Resources, Resource.Drawable.photo_default), true);
            Rep.Instance.Players.Add("Marina", BitmapFactory.DecodeResource(activity.Resources, Resource.Drawable.photo_default), true);
            if (Rep.Instance.Tasks != null)
                return;
            await activity.CreateDataBaseAsync();
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

        public static async Task CreateDataBaseAsync(this Activity context)
        {
            var path = Rep.Instance.DataBaseFile;
            if (!File.Exists(path))
            {
                var connection = new SQLiteAsyncConnection(path);
                var results = await connection.CreateTablesAsync<TaskTable, BonusTable, CategoryTable>();
                if (results.Results.Count == 3)
                {
                    var tasks = GetTasks(context.Assets.Open("Task.csv"), 74);
                    var bonuses = GetBonus(context.Assets.Open("Bonus.csv"), 21);
                    var category = GetCategory(context.Assets.Open("Category.csv"));

                    await connection.InsertAllAsync(tasks);
                    await connection.InsertAllAsync(bonuses);
                    await connection.InsertAllAsync(category);
                }
            }
            await Rep.Instance.TaskGenerateAsync();
            await Rep.Instance.BonusGenerateAsync();
        }

        public static async void AddTask(this Activity context, int count)
        {
            if (!(context is StoreActivity)) return;
            var path = Rep.Instance.DataBaseFile;
            var connection = new SQLiteAsyncConnection(path);
            var tasks = GetTasks(context.Assets.Open("Task.csv"), count);
            await connection.InsertAllAsync(tasks);
            await Rep.Instance.TaskGenerateAsync();
        }

        public static async void AddBonus(this Activity context, int count)
        {
            if (!(context is StoreActivity)) return;
            var path = Rep.Instance.DataBaseFile;
            var connection = new SQLiteAsyncConnection(path);
            var bonuses = GetBonus(context.Assets.Open("Bonus.csv"), count);
            await connection.InsertAllAsync(bonuses); 
            await Rep.Instance.BonusGenerateAsync();
        }

        static IEnumerable<TaskTable> GetTasks(Stream stream, int count)
        {
            var list = new List<TaskTable>();
            using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
            {
                var taskCount = Rep.Instance.Tasks == null ? 0 : Rep.Instance.Tasks.Count;
                for (var index = 0; index < taskCount + count; index++)
                {
                    var line = reader.ReadLine();
                    if (index < taskCount || line == null) continue;
                    var values = line.Split(';');
                    list.Add(new TaskTable(int.Parse(values[0]), values[1], int.Parse(values[2])));
                }
            }
            list.Shuffle();
            return list;
        }

        static IEnumerable<CategoryTable> GetCategory(Stream stream)
        {
            var list = new List<CategoryTable>();
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null) continue;
                    var values = line.Split(';');
                    list.Add(new CategoryTable(int.Parse(values[0]), values[1], values[2]));
                }
            }
            return list;
        }

        private static IEnumerable<BonusTable> GetBonus(Stream stream, int count)
        {
            var list = new List<BonusTable>();
            using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
            {
                var bonusCount = Rep.Instance.Bonuses == null ? 0 : Rep.Instance.Bonuses.Count;
                for (var index = 0; index < bonusCount + count; index++)
                {
                    var line = reader.ReadLine();
                    if (index < bonusCount || line == null) continue;
                    var values = line.Split(';');
                    list.Add(new BonusTable(values[0], int.Parse(values[1])));
                }
            }
            return list;
        }

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
            };
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

        public static void MakeButtonEnabled(this Activity activity, Button button, bool enabled)
        {
            button.Enabled = enabled;
            button.Background.Alpha = enabled ? 255 : 50;
            button.SetTextColor(activity.Resources.GetColor(enabled
                ? Resource.Color.white
                : Resource.Color.pressed_text_color));
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