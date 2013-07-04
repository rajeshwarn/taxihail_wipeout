
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MK.Booking.PayPal
{
    public class ExpressCheckoutServiceClient
    {
        const string ApiVersion = "104";
        readonly Urls _urls;
        readonly UserIdPasswordType _credentials;
        readonly CurrencyCodeType _currency;
        readonly string _returnUrl;
        readonly string _cancelUrl;


        public ExpressCheckoutServiceClient(IPayPalCredentials credentials, RegionInfo region, string returnUrl, string cancelUrl, bool useSandbox = false)
        {
            if (credentials == null) throw new ArgumentNullException("credentials");
            if (region == null) throw new ArgumentNullException("region");
            if (returnUrl == null) throw new ArgumentNullException("returnUrl");
            if (cancelUrl == null) throw new ArgumentNullException("cancelUrl");
            if (string.IsNullOrWhiteSpace(returnUrl)) throw new ArgumentException("returnUrl");
            if (string.IsNullOrWhiteSpace(cancelUrl)) throw new ArgumentException("returnUrl");

            _returnUrl = returnUrl;
            _cancelUrl = cancelUrl;
            _urls = new Urls(useSandbox);
            _currency = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), region.ISOCurrencySymbol);
            _credentials = new UserIdPasswordType
            {
                Username = credentials.Username,
                Password = credentials.Password,
                Signature = credentials.Signature,
            };
        }

        internal string ISO4217CurrencySymbol
        {
            get { return _currency.ToString(); }
        }

        public string SetExpressCheckout(decimal orderTotal)
        {
            using (var api = CreateApiClient())
            {
                var request = BuildRequest(orderTotal);
                var response = api.SetExpressCheckout(request);

                ThrowIfError(response);

                return _urls.GetCheckoutUrl(response.Token);
            }
        }

        private static void ThrowIfError(SetExpressCheckoutResponseType response)
        {
            Debug.Assert(response.Ack == AckCodeType.Success);
            Debug.Assert(response.Token != null);
            foreach (var error in response.Errors)
            {
                Trace.WriteLine(error.LongMessage);
            }
            if (response.Token == null)
            {
                throw new InvalidOperationException("token should not be null");
            }
        }

        private PayPalAPIAASoapBinding CreateApiClient()
        {
            var securityHeader = new CustomSecurityHeaderType
                                     {
                                         Credentials = _credentials
                                     };
            var api = new PayPalAPIAASoapBinding
                          {
                              Url = _urls.GetApiUrl(),
                              RequesterCredentials = securityHeader,
                          };
            return api;
        }

        private SetExpressCheckoutReq BuildRequest(decimal orderTotal)
        {
            var amount = new BasicAmountType
            {
                Value = orderTotal.ToString(CultureInfo.InvariantCulture),
                currencyID = _currency,
            };

            var requestDetails = new SetExpressCheckoutRequestDetailsType
            {
                OrderTotal = amount,
                PaymentAction = PaymentActionCodeType.Sale,
                ReturnURL = _returnUrl,
                CancelURL = _cancelUrl,
            };

            var requestType = new SetExpressCheckoutRequestType
            {
                Version = ApiVersion,
                SetExpressCheckoutRequestDetails = requestDetails,
            };

            var request = new SetExpressCheckoutReq
            {
                SetExpressCheckoutRequest = requestType,
            };

            return request;
        }

        private class Urls
        {
            const string SandboxApiUrl = "https://api-3t.sandbox.paypal.com/2.0/";
            const string ProductionApiUrl = "https://api-3t.paypal.com/2.0/ ";
            const string CheckoutUrlFormat = "https://www.paypal.com/cgi-bin/webscr?cmd=_express-checkout-mobile&token={0}";
            const string SandboxCheckoutUrlFormat = "https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_express-checkout-mobile&token={0}";
            readonly bool _useSandbox;

            public Urls(bool useSandbox)
            {
                _useSandbox = useSandbox;
            }

            public string GetApiUrl()
            {
                return _useSandbox
                            ? SandboxApiUrl
                            : ProductionApiUrl;
            }

            public string GetCheckoutUrl(string token)
            {
                var urlFormat = _useSandbox
                                    ? SandboxCheckoutUrlFormat
                                    : CheckoutUrlFormat;

                return string.Format(urlFormat, Uri.EscapeDataString(token));
            }
        }
    }

    public interface IPayPalCredentials
    {
        string Username { get; }
        string Password { get; }
        string Signature { get; }
    }
}
