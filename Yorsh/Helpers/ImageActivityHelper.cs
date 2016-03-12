using System;
using System.Reflection;
using Android.Graphics;
using Android.Util;
using Yorsh.Data;

namespace Yorsh.Helpers
{
    public class ImageActivityHelper
    {
        private readonly Func<int, TaskTable> _getTaskById;
        private readonly Func<int, CategoryTable> _getCategoryById;
        private readonly int _deviceWidth;
        private readonly int _deviceHeight;
        private const int ImageHeight = 1136;
        private const int ImageWidth = 640;
        private int? _taskId = null;
        private CategoryTable _category;

        public ImageActivityHelper(Func<int, TaskTable> getTask, Func<int, CategoryTable> getCategory, DisplayMetrics displayMetrics)
        {
            _getTaskById = getTask;
            _getCategoryById = getCategory;
			var width = displayMetrics.WidthPixels;
			var height = displayMetrics.HeightPixels;
            _deviceWidth = width <= height ? width : height;
            _deviceHeight = width <= height ? height : width;
        }

        public int? TaskId
        {
            get { return _taskId; }
            set
            {
                if (value == null) throw new NullReferenceException("Task Id cannot set to null value");
                if (_taskId == value) return;
                _taskId = value;
                Task = _getTaskById(_taskId.Value);
                Category = _getCategoryById(Task.CategoryId);
            }
        }

        public TaskTable Task { get; private set; }

        public CategoryTable Category
        {
            get { return _category; }
            private set
            {
                try
                {
                    if (_category != null && _category.Id == value.Id) return;
                    _category = value;
                    using (var resourceStream = ResourceLoader.GetEmbeddedResourceStream(Assembly.GetAssembly(typeof(ResourceLoader)), _category.ImageName))
                    {
                        CategoryImage = BitmapExtensions.DecodeStream(resourceStream, ImageWidth, ImageHeight, _deviceWidth, _deviceHeight);

                        resourceStream.Flush();
                        resourceStream.Dispose();
                    }
                }
                catch (Exception exception)
                {
                    GaService.TrackAppException("ImageActivityHelper", "Category", exception, false);
                }

            }
        }

        public Bitmap CategoryImage
        {
            get; set;
        }

    }
}