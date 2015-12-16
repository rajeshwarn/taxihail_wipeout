using System;

namespace apcurium.MK.Booking.Services
{
    public interface IIbsOrderService
    {
        void ConfirmExternalPayment(Guid orderId, int ibsOrderId, decimal totalAmount, decimal tipAmount, decimal meterAmount, string type, string provider, string transactionId, 
            string authorizationCode, string cardToken, int accountId, string name, string phone, string email, string os, string userAgent, string companyKey, decimal fareAmount = 0, 
            decimal extrasAmount = 0, decimal vatAmount = 0, decimal discountAmount = 0, decimal tollAmount = 0, decimal surchargeAmount = 0);

        void SendPaymentNotification(double totalAmount, double meterAmount, double tipAmount, string authorizationCode, string vehicleNumber, string companyKey = null);

        void SendMessageToDriver(string message, string vehicleNumber, string companyKey);
    }
}
