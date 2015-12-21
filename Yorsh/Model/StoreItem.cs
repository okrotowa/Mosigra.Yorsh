using Xamarin.InAppBilling;

namespace Yorsh.Model
{
    public sealed class StoreItem
    {
        public StoreItem(Product product, bool isSale = false)
        {
            Product = product;
            var splitProductId = Product.ProductId.Split('_');
            int count;
            CountForSort = int.TryParse(splitProductId[0], out count) ? count : 1000;
            BuyElement = splitProductId[1];
            IsSale = isSale;
            ImageString = string.Format("shop_{0}_{1}", BuyElement, splitProductId[0]);
            SaleImageString = isSale ? BuyElement + "_sale" : null;
        }

        public Product Product { get; private set; }
        public int CountForSort { get; private set; }
        public string BuyElement { get; private set; }
        public bool IsSale { get; private set; }
        public string ImageString { get; private set; }
        public string SaleImageString { get; private set; }
    }
}