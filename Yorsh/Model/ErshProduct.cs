using Xamarin.InAppBilling;
using Yorsh.Helpers;

namespace Yorsh.Model
{
    public class ErshProduct
    {
        public ErshProduct(Product product)
        {
            ProductId = product.ProductId;
            Type = product.ProductId.Split('_')[1];
            int count;
            Sale = int.TryParse(product.ProductId.Split('_')[0], out count) ? GetSaleForProduct(count, Type) : 0;
            int price;
            Price = int.TryParse(product.Price_Amount_Micros, out price)
                ? price / 1000000 + " " + product.Price_Currency_Code
                : product.Price;
        }

        public string ProductId { get; private set; }
        public string Type { get; private set; }
        public string Price { get; private set; }
        public bool IsEnabled { get; set; }
        public int Sale { get; private set; }
        public string EnableProductId { get; set; }

        private int GetSaleForProduct(int count, string type)
        {
            switch (type)
            {
                case StringConst.Bonus:
                    return GetSaleForBonus(count);
                case StringConst.Task:
                    return GetSaleForTask(count);
                default:
                    return 0;
            }
        }
       private int GetSaleForBonus(int count)
        {
            switch (count)
            {
                case 30:
                    return 25;
                default:
                    return 0;
            }
        }

        private int GetSaleForTask(int count)
        {
            switch (count)
            {
                case 30:
                    return 25;
                case 70:
                    return 50;
                case 100:
                    return 70;
                default:
                    return 0;
            }
        }
    }
}