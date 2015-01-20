using Foundation;

namespace PaypalSdkTouch
{
	public partial class PayPalMobile
	{
        public static void WithClientIds(NSString productionClientId, NSString sandboxClientId = null)
		{
			var values = new NSObject[] { new NSString(productionClientId), new NSString(sandboxClientId ?? productionClientId) };
			var keys = new NSObject[] { PayPalMobile.PayPalEnvironmentProduction, PayPalMobile.PayPalEnvironmentSandbox };

			var clientIds = NSDictionary.FromObjectsAndKeys(values, keys);
			PayPalMobile.InitializeWithClientIdsForEnvironments(clientIds);
		}
	}
}