using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;
using Xamarin.InAppBilling.Model;
using Yorsh.Helpers;
using Yorsh.Model;

namespace Yorsh.Adapters
{
    public sealed class StoreListAdapter : BaseAdapter<StoreItem>
    {
        readonly Activity _context;
        readonly List<StoreItem> _products;
        public StoreListAdapter(Activity context, IEnumerable<Product> products, Func<Product, bool> productIsSale)
        {
            _context = context;
            var productList = products.Select(prod => new StoreItem(prod, productIsSale(prod))).ToList();
            _products = productList.OrderBy(item => item.CountForSort).ToList();
        }

        public override StoreItem this[int position]
        {
            get { return _products[position]; }
        }


        public override int Count
        {
            get { return _products.Count(); }
        }

        public IList<StoreItem> Items
        {
            get { return _products; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView != null) return convertView;
            var storeItem = this[position];
            var view = _context.LayoutInflater.Inflate(Resource.Layout.StoreItem, null);
            var button = view.FindViewById<ImageButton>(Resource.Id.storeButton);
            var drawable = _context.Resources.GetIdentifier(storeItem.ImageString, "drawable", _context.PackageName);
            button.SetImageResource(drawable);

            button.Click += (sender, e) =>
            {
                //buy something
            };
            var saleImage = view.FindViewById<ImageView>(Resource.Id.saleImageView);
            saleImage.Visibility = storeItem.IsSale ? ViewStates.Visible : ViewStates.Invisible;
            if (storeItem.IsSale) saleImage.SetImageResource(_context.Resources.GetIdentifier(storeItem.SaleImageString, "drawable", _context.PackageName));

            var priceText = view.FindViewById<TextView>(Resource.Id.priceText);
            priceText.SetTypeface(_context.MyriadProFont(MyriadPro.SemiboldCondensed), Android.Graphics.TypefaceStyle.Normal);
            priceText.Text = storeItem.Product.Price;

            return view;
        }
    }
}

