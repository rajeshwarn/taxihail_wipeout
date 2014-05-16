using System;
namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public interface IIbsOrderService
    {
        void ConfirmExternalPayment(Guid orderId, int ibsOrderId, decimal totalAmount, decimal tipAmount, decimal meterAmount, string type, string provider, string transactionId,
           string authorizationCode, string cardToken, int accountID, string name, string phone, string email, string os, string userAgent);

        void SendPaymentNotification(string message, string vehicleNumber, int ibsOrderId);
    }
}