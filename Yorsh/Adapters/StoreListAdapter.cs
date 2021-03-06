using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Yorsh.Data;
using Yorsh.Listeners;
using Yorsh.Model;
using Yorsh.Model.EventAgruments;

namespace Yorsh.Adapters
{
    public sealed class StoreListAdapter : BaseAdapter<StoreItem>
    {
        readonly Activity _context;
        readonly List<StoreItem> _products;

        public StoreListAdapter(Activity context, IEnumerable<ErshProduct> products)
        {
            _context = context;
            var productList = products.Select(prod => new StoreItem(
                prod, prod.Sale)).ToList();
            _products = productList.OrderBy(item => item.CountForSort).ToList();
        }
        
        public override bool IsEnabled(int position)
        {
            return this[position].Product.IsEnabled;
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
            var storeButton = view.FindViewById<ImageButton>(Resource.Id.storeButton);
            var saleImage = view.FindViewById<ImageView>(Resource.Id.saleImageView);
            var priceText = view.FindViewById<TextView>(Resource.Id.priceText);
            priceText.SetTypeface(Rep.FontManager.Get(Font.SemiboldCondensed), TypefaceStyle.Normal);

            var drawable = _context.Resources.GetIdentifier(storeItem.ImageString, "drawable", _context.PackageName);
            storeButton.SetImageResource(drawable);

            storeButton.Enabled = storeItem.Product.IsEnabled;
            storeButton.ImageAlpha = storeButton.Enabled ? 255 : 145;
            storeButton.SetOnClickListener(new StoreButtonClickListener(this[position].Product, OnItemClick));

            saleImage.Visibility = storeItem.IsSale
                ? ViewStates.Visible
                : ViewStates.Invisible;
            if (storeItem.IsSale) saleImage.SetImageResource(_context.Resources.GetIdentifier(storeItem.SaleImageString, "drawable", _context.PackageName));

            priceText.SetTypeface(Rep.FontManager.Get(Font.SemiboldCondensed), TypefaceStyle.Normal);
			priceText.Text = storeItem.Product.Price;

            return view;
        }

    }
}

