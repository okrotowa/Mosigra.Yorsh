using System;
using Android.Views;
using Yorsh.Model;
using Yorsh.Model.EventAgruments;

namespace Yorsh.Listeners
{
    public class StoreButtonClickListener : Java.Lang.Object, View.IOnClickListener
    {
        private readonly ErshProduct _product;
        private readonly Action<StoreItemClickEventArgs> _onItemClick;

        public StoreButtonClickListener(ErshProduct product, Action<StoreItemClickEventArgs> onItemClick)
        {
            _product = product;
            _onItemClick = onItemClick;
        }

        public void OnClick(View v)
        {
            _onItemClick(new StoreItemClickEventArgs(_product));
        }

    }
}