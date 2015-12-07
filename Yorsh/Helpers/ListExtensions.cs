using System;
using System.Collections.Generic;
using Android.Widget;
using Android.Views;

namespace Yorsh.Helpers
{
    public static class ListExtensions
    {
		public static void JustifyListViewHeightBasedOnChildren (this ListView listView) 
		{
			var adapter = listView.Adapter;
			if (adapter == null) {
				return;
			}
			int totalHeight = 0;
			for (int i = 0; i < adapter.Count; i++)
			{
				View listItem = adapter.GetView(i, null, listView);
				listItem.Measure(0, 0);
				totalHeight += listItem.MeasuredHeight;
			}

			var par = listView.LayoutParameters;
			par.Height = totalHeight + (listView.DividerHeight * (adapter.Count - 1));
			listView.LayoutParameters = par;
			listView.RequestLayout ();
		}

        public static void Shuffle<T>(this IList<T> list)
        {
            var random = new Random();
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}