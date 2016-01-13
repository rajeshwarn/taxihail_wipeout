using System;
using Cirrious.CrossCore.Droid.Platform;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using Braintree.Api;
using Braintree.Api.Models;
using BTCard = Braintree.Api.Card;

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

	        var resultTask = fragment.GetPaymentMethodNonceTask();
            
            PayPal.AuthorizeAccount(fragment,new []
            {
                "profile"
            });

	        var request = new PayPalRequest().ShippingAddressRequired(false);

            PayPal.RequestBillingAgreement(fragment, request);

	        var result = await resultTask;

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

            var resultTask = fragment.GetPaymentMethodNonceTask();

            BTCard.Tokenize(fragment, cardBuilder);

	        var result = await resultTask;

            return result.Nonce;
	    }

	    public Task<string> GetPlatformPayNone(string clientToken)
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

