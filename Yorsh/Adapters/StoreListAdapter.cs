using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Xamarin.InAppBilling;
using Yorsh.Helpers;
using Yorsh.Model;

namespace Yorsh.Adapters
{
    public sealed class StoreListAdapter : BaseAdapter<StoreItem>
    {
        readonly Activity _context;
        readonly List<StoreItem> _products;
        private readonly Func<string, bool> _productIsEnabled;

		public StoreListAdapter(Activity context, IEnumerable<Product> products,Func<string,int> saleForProduct, Func<string, bool> productIsEnabled)
        {
            _context = context;
            var productList = products.Select(prod => new StoreItem(
                prod, saleForProduct(prod.ProductId))).ToList();
            _products = productList.OrderBy(item => item.CountForSort).ToList();
            _productIsEnabled = productIsEnabled;
        }
        public override bool IsEnabled(int position)
        {
            return _productIsEnabled(this[position].Product.ProductId);
        }

        public event EventHandler<StoreItemClickEventArgs> ItemClick;

        private void OnItemClick(StoreItemClickEventArgs e)
        {
            var handler = ItemClick;
            if (handler != null) handler(this, e);
        }

        public override StoreItem this[int position]
        {
            get { return _products[position]; }
        }


        public override int Count
        {
            get { return _products.Count(); }
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
            view.Clickable = true;
            var button = view.FindViewById<ImageButton>(Resource.Id.storeButton);
            var drawable = _context.Resources.GetIdentifier(storeItem.ImageString, "drawable", _context.PackageName);
            button.SetImageResource(drawable);

            button.Enabled = _productIsEnabled(storeItem.Product.ProductId);
            button.ImageAlpha = button.Enabled ? 255 : 145;
            button.Click += (sender, args) => OnItemClick(new StoreItemClickEventArgs(this[position].Product));
            var saleImage = view.FindViewById<ImageView>(Resource.Id.saleImageView);
            saleImage.Visibility = storeItem.IsSale
                ? ViewStates.Visible
                : ViewStates.Invisible;
            if (storeItem.IsSale) saleImage.SetImageResource(_context.Resources.GetIdentifier(storeItem.SaleImageString, "drawable", _context.PackageName));

            var priceText = view.FindViewById<TextView>(Resource.Id.priceText);
            priceText.SetTypeface(_context.MyriadProFont(MyriadPro.SemiboldCondensed), TypefaceStyle.Normal);
			int price; 
			priceText.Text = int.TryParse (storeItem.Product.Price_Amount_Micros, out price) 
				? price / 1000000 + " " + storeItem.Product.Price_Currency_Code
				: storeItem.Product.Price;

            return view;
        }

    }
}

