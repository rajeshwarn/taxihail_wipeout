using System;

namespace apcurium.MK.Booking.Services
{
    public interface IIbsOrderService
    {
        void ConfirmExternalPayment(Guid orderId, int ibsOrderId, decimal totalAmount, decimal tipAmount, decimal meterAmount, string type, string provider, string transactionId,
           string authorizationCode, string cardToken, int accountID, string name, string phone, string email, string os, string userAgent, string companyKey = null);

        void SendPaymentNotification(double totalAmount, double meterAmount, double tipAmount, string authorizationCode, string vehicleNumber, string companyKey = null);
        
        void SendMessageToDriver(string message, string vehicleNumber, string companyKey = null);
    }
}
