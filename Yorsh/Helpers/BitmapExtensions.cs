using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Math = Java.Lang.Math;

namespace Yorsh.Helpers
{
    public static class BitmapExtensions
    {
        public static Task<Bitmap> DecodeBitmapAsync(string path, int desiredWidth, int desiredHeight)
        {
            return Task.Factory.StartNew(() => DecodeBitmap(path, desiredWidth, desiredHeight));
        }


        //public static Task<Bitmap> DecodeBitmapAsync(string path,  int desiredWidth, int desiredHeight)
        // {
        //     return Task.Factory.StartNew(() => DecodeBitmap(path, desiredSize));
        // }

        public static Bitmap DecodeStream(Stream stream,int width, int height, int desiredWidth, int desiredHeight)
        {
            var sampleSize = 1;
            if (height > desiredHeight || width > desiredWidth)
            {
                var heightRatio = (int)Math.Round((float)height / (float)desiredHeight);
                var widthRatio = (int)Math.Round((float)width / (float)desiredWidth);
                sampleSize = Math.Min(heightRatio, widthRatio);
            }
            var options = new BitmapFactory.Options { InSampleSize = sampleSize };
            return BitmapFactory.DecodeStream(stream, null, options);
        }
        public static Bitmap DecodeBitmap(string path, int desiredSize)
        {
            var options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(path, options);
            options = new BitmapFactory.Options { InSampleSize = desiredSize };
            return BitmapFactory.DecodeFile(path, options);
        }

        public static Bitmap DecodeBitmap(string path, int desiredWidth, int desiredHeight)
        {
            var options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(path, options);

            var height = options.OutHeight;
            var width = options.OutWidth;

            var sampleSize = 1;
            if (height > desiredHeight || width > desiredWidth)
            {
                var heightRatio = (int)Math.Round((float)height / (float)desiredHeight);
                var widthRatio = (int)Math.Round((float)width / (float)desiredWidth);
                sampleSize = Math.Min(heightRatio, widthRatio);
            }
            options = new BitmapFactory.Options { InSampleSize = sampleSize };
            return BitmapFactory.DecodeFile(path, options);
        }

        public static Bitmap GetRoundedCornerBitmap(this Bitmap bitmap, int? roundPixelSize = null)
        {
            var chooseSize = bitmap.Width > bitmap.Height ? bitmap.Height : bitmap.Width;
            roundPixelSize = roundPixelSize ?? (int)Application.Context.Resources.GetDimension(Resource.Dimension.RoundedCorners);
            var output = Bitmap.CreateBitmap(chooseSize, chooseSize, Bitmap.Config.Argb8888);
            var canvas = new Canvas(output);
            var paint = new Paint();
            var rect = new Rect(0, 0, chooseSize, chooseSize);
            var rectF = new RectF(rect);
            var roundPx = roundPixelSize.Value;
            paint.AntiAlias = true;
            canvas.DrawRoundRect(rectF, roundPx, roundPx, paint);
            paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
            canvas.DrawBitmap(bitmap, rect, rect, paint);
            return output;
        }
    }
}

