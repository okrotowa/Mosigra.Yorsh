using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;
using Java.IO;
using Java.Lang;

namespace Yorsh.Helpers
{
    public static class BitmapExtensions
    {
        public static Task<Bitmap> DecodeBitmapAsync(string path, int desiredWidth, int desiredHeight)
        {
            return Task.Factory.StartNew(() =>
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
                });
        }

        public static byte[] ToByteArray(this Bitmap image)
        {
            byte[] byteArray = null;
            using ( var stream = new MemoryStream())
            {
                image.Compress(Bitmap.CompressFormat.Png, 100, stream);
                byteArray = stream.ToArray();
            }
            return byteArray;
        }

        public static Bitmap GetRoundedCornerBitmap(this Bitmap bitmap, int roundPixelSize)
        {
            var output = Bitmap.CreateBitmap(bitmap.Width, bitmap.Height, Bitmap.Config.Argb8888);
            var canvas = new Canvas(output);
            var paint = new Paint();
            var rect = new Rect(0, 0, bitmap.Width, bitmap.Height);
            var rectF = new RectF(rect);
            var roundPx = roundPixelSize;
            paint.AntiAlias = true;
            canvas.DrawRoundRect(rectF, roundPx, roundPx, paint);
            paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
            canvas.DrawBitmap(bitmap, rect, rect, paint);
            return output;
        }
    }
}

