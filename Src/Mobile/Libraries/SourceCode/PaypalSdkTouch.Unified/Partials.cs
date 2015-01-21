using System;
using Foundation;

namespace PaypalSdkTouch.Unified
{
    public partial class PayPalMobile
    {
        public static void WithClientIds(NSString productionClientId, NSString sandboxClientId = null)
        {
            var values = new NSObject[] { productionClientId, sandboxClientId ?? productionClientId };
            var keys = new NSObject[] { PayPalMobile.PayPalEnvironmentProduction, PayPalMobile.PayPalEnvironmentSandbox };

            var clientIds = NSDictionary.FromObjectsAndKeys(values, keys);
            PayPalMobile.InitializeWithClientIdsForEnvironments(clientIds);
        }
    }
}

