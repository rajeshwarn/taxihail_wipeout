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

        public IbsOrderService(IBookingWebServiceClient client)
        {
            _client = client;
        }

        public void ConfirmExternalPayment(int orderId, string vehicleId, string text, double amount, double fareAmount, string cardType, string cardNumber, string cardExpiry, string transactionId, string authorizationCode)
        {
            if ( !_client.ConfirmExternalPayment(orderId, vehicleId, text, amount, fareAmount, cardType, cardNumber, cardExpiry, transactionId, authorizationCode) )
            {
                throw new Exception("Cannot confirm the payment");
            }
        }

        public void SendPaymentNotification(string message, string vehicleNumber, int ibsOrderId)
        {

            if ( !_client.SendPaymentNotification(message, vehicleNumber, ibsOrderId) )
            {
                throw new Exception("Cannot send the payment notification");
            }
        }
    }
}