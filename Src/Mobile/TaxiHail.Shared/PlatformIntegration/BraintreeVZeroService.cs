using System;
using Cirrious.CrossCore.Droid.Platform;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using Braintree.Api;
using Braintree.Api.Models;

namespace TaxiHail.Shared.PlatformIntegration
{
	public class BraintreeVZeroService : IPaymentProviderClientService
    {
		private readonly IMvxAndroidCurrentTopActivity _activity;

		public BraintreeVZeroService(IMvxAndroidCurrentTopActivity activity)
		{
		    _activity = activity;
		}

	    public async Task<string> GetPayPalNonce(string clientToken)
	    {
	        var fragment = GenerateBraintreeFragment(clientToken);

	        var additionalParams = new[]
	        {
	            "profile"
	        };

            var request = new PayPalRequest().ShippingAddressRequired(false);

            var result = await fragment.AuthorizePaypalPaymentAsync(true, additionalParams, request).ConfigureAwait(false);

	        return result.Nonce;
	    }

	    public async Task<string> GetCreditCardNonce(string clientToken, string creditCardNumber, string ccv, string expirationMonth, string expirationYear, string firstName, string lastName, string zipCode)
	    {
            var fragment = GenerateBraintreeFragment(clientToken);
	        
            var cardBuilder = new CardBuilder()
	            .CardNumber(creditCardNumber)
	            .Cvv(ccv)
                .ExpirationMonth(expirationMonth)
                .ExpirationYear(expirationYear)
                .FirstName(firstName)
                .LastName(lastName);

	        if (zipCode.HasValueTrimmed())
	        {
	            cardBuilder = cardBuilder.PostalCode(zipCode);
	        }

	        var result = await fragment.TokenizeAsync(cardBuilder).ConfigureAwait(false);

            return result.Nonce;
	    }

        public Task<string> GetPlatformPayNonce(string clientToken)
	    {
	        throw new NotImplementedException("Android pay is not currently implemented.");
	    }

	    private BraintreeFragment GenerateBraintreeFragment(string clientToken)
	    {
            var activity = _activity.Activity;

            return BraintreeFragment.NewInstance(activity, clientToken);
        }
    }
}

