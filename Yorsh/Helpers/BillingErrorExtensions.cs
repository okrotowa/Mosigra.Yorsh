using System;
using Android.OS;
using Xamarin.InAppBilling;

namespace Yorsh.Helpers
{
    public static class BillingErrorExtensions
    {
        public static void UnbindErrors(this InAppBillingHandler handler)
        {
            handler.BuyProductError -= HandlerOnBuyProductError;
            handler.InAppBillingProcesingError -= HandlerOnInAppBillingProcesingError;
            handler.OnGetProductsError -= HandlerOnOnGetProductsError;
            handler.OnInvalidOwnedItemsBundleReturned -= HandlerOnOnInvalidOwnedItemsBundleReturned;
            handler.OnProductPurchasedError -= HandlerOnOnProductPurchasedError;
            handler.OnPurchaseConsumedError -= HandlerOnOnPurchaseConsumedError;
            handler.OnPurchaseFailedValidation -= HandlerOnOnPurchaseFailedValidation;
        }

        public static void BindErrors(this InAppBillingHandler handler)
        {
            handler.BuyProductError += HandlerOnBuyProductError;
            handler.InAppBillingProcesingError += HandlerOnInAppBillingProcesingError;
            handler.OnGetProductsError += HandlerOnOnGetProductsError;
            handler.OnInvalidOwnedItemsBundleReturned += HandlerOnOnInvalidOwnedItemsBundleReturned;
            handler.OnProductPurchasedError += HandlerOnOnProductPurchasedError;
            handler.OnPurchaseConsumedError += HandlerOnOnPurchaseConsumedError;
            handler.OnPurchaseFailedValidation += HandlerOnOnPurchaseFailedValidation;
        }

        private static void HandlerOnOnPurchaseFailedValidation(Purchase purchase, string purchaseData, string purchaseSignature)
        {
            var message = string.Format("Id={0}, developerPayload={1}, state={2}, orderId={3}, time={4}, data={5}", 
                purchase.ProductId, purchase.DeveloperPayload, purchase.PurchaseState,
                purchase.OrderId, purchase.PurchaseTime, purchaseData);
            GaService.TrackAppException("Extensions", "HandlerOnOnPurchaseFailedValidation", string.Empty, message, false);
        }

        private static void HandlerOnOnPurchaseConsumedError(int responseCode, string token)
        {
            var obj = Enum.ToObject(typeof(BillingResult), responseCode);
            var exceptionName = obj == null ? "Response code = " + responseCode : obj.ToString();
            var message = "Token = " + token;
            GaService.TrackAppException("Extensions", "HandlerOnOnPurchaseConsumedError", exceptionName, message, false);
        }

        private static void HandlerOnOnProductPurchasedError(int responseCode, string sku)
        {
            var obj = Enum.ToObject(typeof(BillingResult), responseCode);
            var exceptionName = obj == null ? "Response code = " + responseCode : obj.ToString();
            var message = "Sku = " + sku;
            GaService.TrackAppException("Extensions", "HandlerOnOnProductPurchasedError", exceptionName, message, false);
        }

        private static void HandlerOnOnInvalidOwnedItemsBundleReturned(Bundle ownedItems)
        {
            GaService.TrackAppException("Extensions", "HandlerOnOnInvalidOwnedItemsBundleReturned", string.Empty, "Get owned items in bundle", false);
        }

        private static void HandlerOnOnGetProductsError(int responseCode, Bundle ownedItems)
        {
            var obj = Enum.ToObject(typeof(BillingResult), responseCode);
            var exceptionName = obj == null ? "Response code = " + responseCode : obj.ToString();
            GaService.TrackAppException("Extensions", "HandlerOnOnGetProductsError", exceptionName, "Get owned items in bundle", false);
        }

        private static void HandlerOnBuyProductError(int responseCode, string sku)
        {
            var obj = Enum.ToObject(typeof(BillingResult), responseCode);
            var exceptionName = obj == null ? "Response code = " + responseCode : obj.ToString();
            var message = "Sku = " + sku;
            GaService.TrackAppException("Extensions", "HandlerOnBuyProductError", exceptionName, message, false);
        }
        private static void HandlerOnInAppBillingProcesingError(string message)
        {
            GaService.TrackAppException("Extensions", "HandlerOnInAppBillingProcesingError", string.Empty, message, false);
        }

        public static void UnbindErrors(this InAppBillingServiceConnection connection)
        {
            connection.OnInAppBillingError -= ConnectionOnOnInAppBillingError;
        }
        public static void BindErrors(this InAppBillingServiceConnection connection)
        {
            connection.OnInAppBillingError += ConnectionOnOnInAppBillingError;
        }

        private static void ConnectionOnOnInAppBillingError(InAppBillingErrorType error, string message)
        {
            GaService.TrackAppException("Extensions", "ConnectionOnOnInAppBillingError", error.ToString(), message, false);
        }
    }
}