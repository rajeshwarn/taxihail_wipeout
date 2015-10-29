using System;

namespace apcurium.MK.Booking.Services
{
    public interface IIbsOrderService
    {
        void ConfirmExternalPayment(Guid orderId, int ibsOrderId, decimal totalAmount, decimal tipAmount, decimal meterAmount, string type, string provider, string transactionId,
           string authorizationCode, string cardToken, int accountId, string name, string phone, string email, string os, string userAgent, string companyKey);

        void SendPaymentNotification(double totalAmount, double taxedMeterAmount, double tipAmount, string authorizationCode, string vehicleNumber, string companyKey);
        
        void SendMessageToDriver(string message, string vehicleNumber, string companyKey);
    }
}
