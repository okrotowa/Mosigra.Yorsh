using System;
using System.Linq;
using Xamarin.InAppBilling;
using Yorsh.Helpers;

namespace Yorsh.Model
{
    public class ErshPurchase
    {
        private int _count = 0;
        private bool _isAll = false;
        private string _productType;
        private bool _isImplemented = true;

        public ErshPurchase(string purchaseId)
        {
            var splitProductId = purchaseId.Split('_');
            if (splitProductId.Count() == 2)
            {
                if (!int.TryParse(splitProductId[0], out _count))
                {
                    _isAll = splitProductId[0] == StringConst.All;
                    _isImplemented = _isAll;
                }
                _productType = splitProductId[1];
                _isImplemented &= _productType != StringConst.Task || _productType != StringConst.Bonus;
            }
            else
            {
                _isImplemented = false;
            }
        }
        
        public int Count
        {
            get { return _count; }
        }

        public bool IsAll
        {
            get { return _isAll; }
        }

        public string ProductType
        {
            get { return _productType; }
        }

        public bool IsImplemented
        {
            get { return _isImplemented; }
        }

    }
}