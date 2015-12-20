using System;
using Xamarin.InAppBilling.Model;

namespace Yorsh.Model
{
    public class StoreItemClickEventArgs : EventArgs
    {
        private readonly Product _product;
        public StoreItemClickEventArgs(Product product)
        {
            _product = product;
        }

        public Product Product { get { return _product; } }
    }
}