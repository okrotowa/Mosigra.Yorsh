using System;
using System.Collections.Generic;
using System.Linq;
using Android.Widget;
using Android.Views;
using Java.Util.Concurrent;
using Yorsh.Data;

namespace Yorsh.Helpers
{
    public static class ListExtensions
    {
        public static void JustifyListViewHeightBasedOnChildren(this ListView listView)
        {
            var adapter = listView.Adapter;
            if (adapter == null)
            {
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
            listView.RequestLayout();
        }

        private static void Shuffle(this IList<TaskTable> list)
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

        public static IEnumerable<TaskTable> CustomSort(this IEnumerable<TaskTable> list, out int position)
        {
            var positioinList = new List<TaskTable>();
            var nonPositioinList = new List<TaskTable>();

            foreach (var taskTable in list)
            {
                if (taskTable.Position != 0)
                {
                    positioinList.Add(taskTable);
                }
                else
                {
                    nonPositioinList.Add(taskTable);
                }
            }
            nonPositioinList.Shuffle();

            var sortedPositionList = positioinList.OrderBy(x => x.Position);
            var positionListCount = positioinList.Count();
            position = positionListCount > 0 ? positionListCount - 1 : 0;

            return sortedPositionList.Union(nonPositioinList).ToList();
        }
    }
}