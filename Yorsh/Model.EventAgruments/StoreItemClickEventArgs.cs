using System;

namespace Yorsh.Model.EventAgruments
{
    public class StoreItemClickEventArgs : EventArgs
    {
        private readonly ErshProduct _product;
        public StoreItemClickEventArgs(ErshProduct product)
        {
            _product = product;
        }

        public ErshProduct Product { get { return _product; } }
    }
}