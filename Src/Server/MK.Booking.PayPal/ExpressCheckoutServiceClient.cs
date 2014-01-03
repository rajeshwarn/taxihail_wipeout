
using System;
using System.Diagnostics;
using System.Globalization;

using apcurium.MK.Common.Configuration.Impl;

namespace MK.Booking.PayPal
{
    public class ExpressCheckoutServiceClient
    {
        // Documentation:
        // https://developer.paypal.com/webapps/developer/docs/classic/express-checkout/integration-guide/ECGettingStarted/
        // https://developer.paypal.com/webapps/developer/docs/classic/api/
        // ----------------------------------------------------------------------

        const string ApiVersion = "104";
        const string MobileKnowledgeReferralCode = "MobileKnowledgeSystems_SP_MEC";
        readonly Urls _urls;
        readonly UserIdPasswordType _credentials;
        readonly CurrencyCodeType _currency;

        public ExpressCheckoutServiceClient(PayPalCredentials credentials, RegionInfo region, bool useSandbox = false)
        {
            if (credentials == null) throw new ArgumentNullException("credentials");
            if (region == null) throw new ArgumentNullException("region");
            
            _urls = new Urls(useSandbox);
            _currency = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), region.ISOCurrencySymbol);
            _credentials = new UserIdPasswordType
            {
                Username = credentials.Username,
                Password = credentials.Password,
                Signature = credentials.Signature,
            };
        }

// ReSharper disable once InconsistentNaming
        internal string ISO4217CurrencySymbol
        {
            get { return _currency.ToString(); }
        }

        public string SetExpressCheckout(decimal orderTotal, string returnUrl, string cancelUrl)
        {
            if (returnUrl == null) throw new ArgumentNullException("returnUrl");
            if (cancelUrl == null) throw new ArgumentNullException("cancelUrl");
            if (string.IsNullOrWhiteSpace(returnUrl)) throw new ArgumentException("returnUrl");
            if (string.IsNullOrWhiteSpace(cancelUrl)) throw new ArgumentException("returnUrl");

            using (var api = CreateApiClient())
            {
                var request = BuildRequest(orderTotal, returnUrl, cancelUrl);
                var response = api.SetExpressCheckout(request);

                ThrowIfError(response);

                return response.Token;
            }
        }

        public string GetCheckoutUrl(string token)
        {
            return _urls.GetCheckoutUrl(token);
        }

        public string DoExpressCheckoutPayment(string token, string payerId, decimal orderTotal)
        {
            using (var api = CreateApiClient())
            {

                var amount = new BasicAmountType
                {
                    Value = orderTotal.ToString(CultureInfo.InvariantCulture),
                    currencyID = _currency,
                };

                var paymentDetails = new PaymentDetailsType
                {
                    OrderTotal = amount,
                };

                var requestDetails = new DoExpressCheckoutPaymentRequestDetailsType
                {
                    // Important:
                    // ButtonSource is a tracking code that links the tansaction with Mobile Knowledge
                    ButtonSource = MobileKnowledgeReferralCode,
                    PayerID = payerId,
                    Token = token,
                    PaymentDetails = new [] { paymentDetails },
                    PaymentAction = PaymentActionCodeType.Sale,
                };

                var requestType = new DoExpressCheckoutPaymentRequestType
                {
                    Version = ApiVersion,
                    DoExpressCheckoutPaymentRequestDetails = requestDetails,
                };

                var request = new DoExpressCheckoutPaymentReq
                {
                    DoExpressCheckoutPaymentRequest = requestType,
                };

                var response = api.DoExpressCheckoutPayment(request);
                ThrowIfError(response);

                return response.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].TransactionID;
            }
        }

        private static void ThrowIfError(AbstractResponseType response)
        {
            Debug.Assert(response.Ack == AckCodeType.Success);
            
            if (response.Ack != AckCodeType.Success)
            {
                var errors = string.Empty;
                if (response.Errors != null)
                {
                    foreach (var error in response.Errors)
                    {
                        Trace.WriteLine(error.LongMessage);
                        errors += error + Environment.NewLine;
                    }
                }

                throw new ExpressCheckoutException(errors);
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

        private SetExpressCheckoutReq BuildRequest(decimal orderTotal, string returnUrl, string cancelUrl)
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
                ReturnURL = returnUrl,
                CancelURL = cancelUrl,
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


}
