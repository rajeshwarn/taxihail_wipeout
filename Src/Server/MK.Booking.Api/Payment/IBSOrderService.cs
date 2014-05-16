#region

using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.IBS;
using System;

#endregion

namespace apcurium.MK.Booking.Api.Payment
{
    public class IbsOrderService : IIbsOrderService
    {
        private readonly IBookingWebServiceClient _client;

        public IbsOrderService( IBookingWebServiceClient client)
        {
            _client = client;
        }

       
         public void ConfirmExternalPayment(Guid orderId, int ibsOrderId, decimal totalAmount, decimal tipAmount, decimal meterAmount, string type, string provider, string transactionId,
                                                    string authorizationCode, string cardToken, int accountID, string name, string phone, string email, string os, string userAgent)       
        {
            if (!_client.ConfirmExternalPayment(orderId, ibsOrderId, totalAmount, tipAmount, meterAmount, type, provider, transactionId,
                            authorizationCode, cardToken, accountID, name, phone, email, os, userAgent) )
            {
                throw new Exception("Cannot send payment information to dispatch.");
            }
        }

        public void SendPaymentNotification(string message, string vehicleNumber, int ibsOrderId)
        {

            if ( !_client.SendPaymentNotification(message, vehicleNumber, ibsOrderId) )
            {
				throw new Exception("Cannot send the payment notification.");
            }
        }
    }
}