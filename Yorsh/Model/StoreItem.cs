using Xamarin.InAppBilling;

namespace Yorsh.Model
{
    public sealed class StoreItem
    {
		public StoreItem(Product product, int saleProcent = 0)
        {
            Product = product;
            var splitProductId = Product.ProductId.Split('_');
            int count;
            CountForSort = int.TryParse(splitProductId[0], out count) ? count : 1000;
            BuyElement = splitProductId[1];
			IsSale = saleProcent != 0;
            ImageString = string.Format("shop_{0}_{1}", BuyElement, splitProductId[0]);
			SaleImageString = BuyElement + "_sale_" + saleProcent;
        }

        public Product Product { get; private set; }
        public int CountForSort { get; private set; }
        public string BuyElement { get; private set; }
        public bool IsSale { get; private set; }
        public string ImageString { get; private set; }
        public string SaleImageString { get; private set; }
    }
}