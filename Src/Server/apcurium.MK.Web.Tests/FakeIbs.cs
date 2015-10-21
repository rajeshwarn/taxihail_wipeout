using System;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.Services;

namespace apcurium.MK.Web.Tests
{
    public class FakeIbs : IIbsOrderService
    {
        public void ConfirmExternalPayment( Guid orderId, int ibsOrderId, decimal totalAmount, decimal tipAmount, decimal meterAmount, string type,
            string provider, string transactionId, string authorizationCode, string cardToken, int accountId, string name,
            string phone, string email, string os, string userAgent, string companyKey = null)
        {
            if (Fail)
            {
                throw new Exception("ibs failed");
            }
        }

        public void SendPaymentNotification(double totalAmount, double taxedMeterAmount, double tipAmount, string authorizationCode,
            string vehicleNumber, string companyKey = null)
        {
        }

        public void SendMessageToDriver(string message, string vehicleNumber, string companyKey = null)
        {
        }

        public bool Fail { get; set; }
    }
}