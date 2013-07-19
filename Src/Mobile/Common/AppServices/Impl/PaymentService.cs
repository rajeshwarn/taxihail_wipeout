using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Views;
using SocialNetworks.Services;
using apcurium.MK.Booking.Mobile.Data;
using MK.Booking.Api.Client;
using ServiceStack.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client.Payments.Braintree;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

#if IOS
using ServiceStack.ServiceClient.Web;
using ServiceStack.Common.ServiceClient.Web;
#endif
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using System.Net;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class PaymentService : BaseService, IPaymentService
    {
        IConfigurationManager _configurationManager;
        string _baseUrl; 
        string _sessionId;


        public PaymentService(string url, string sessionId,  IConfigurationManager configurationManager)
        {
            _baseUrl = url;
            _sessionId = sessionId;
            _configurationManager = configurationManager;
        }

        
        public bool IsOrderPayed(Guid orderId)
        {
            throw new NotImplementedException();
        }

        public IPaymentServiceClient GetClient()
        {
            const string onErrorMessage = "Payment Method not found or unknown";

            var settings = _configurationManager.GetPaymentSettings();

            var braintreeServiceClient = new BraintreeServiceClient(_baseUrl,_sessionId,settings.BraintreeClientSettings.ClientKey);
            var cmtServiceClient = new CmtPaymentClient(_baseUrl,_sessionId, settings.CmtPaymentSettings);

            switch (settings.PaymentMode)
            {
                case PaymentMethod.Braintree:
                    return braintreeServiceClient;

                    case PaymentMethod.Cmt:
                    return cmtServiceClient;

                    case PaymentMethod.Fake:
                    return new FakePaymentClient();
                    default:
                    throw new Exception(onErrorMessage);
            }
        }



        public TokenizedCreditCardResponse Tokenize(string creditCardNumber, DateTime expiryDate, string cvv)
        {
            return GetClient().Tokenize(creditCardNumber, expiryDate, cvv);
        }

        public DeleteTokenizedCreditcardResponse ForgetTokenizedCard(string cardToken)
        {
            return GetClient().ForgetTokenizedCard(cardToken);
        }

        public PreAuthorizePaymentResponse PreAuthorize(string cardToken, double amount, Guid orderId)
        {
            return GetClient().PreAuthorize(cardToken, amount, orderId);
        }

        public CommitPreauthorizedPaymentResponse CommitPreAuthorized(string transactionId)
        {
            return GetClient().CommitPreAuthorized(transactionId);
        }   


    }
}


