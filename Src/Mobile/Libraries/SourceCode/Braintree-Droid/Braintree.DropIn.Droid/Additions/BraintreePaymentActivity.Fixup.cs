using Android.Runtime;

namespace Com.Braintreepayments.Api
{
    public partial class BraintreePaymentActivity
    {
        [Register("EXTRA_CLIENT_TOKEN")]
        public const string EXTRA_CLIENT_TOKEN = "com.braintreepayments.api.dropin.EXTRA_CLIENT_TOKEN";
    }
}