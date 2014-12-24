using System;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.Services;

namespace apcurium.MK.Web.Tests
{
    public class FakeIbs : IIbsOrderService
    {
        public void ConfirmExternalPayment( Guid orderId, int ibsOrderId, decimal totalAmount, decimal tipAmount, decimal meterAmount, string type,
            string provider, string transactionId, string authorizationCode, string cardToken, int accountID, string name,
            string phone, string email, string os, string userAgent)
        {
            if (Fail)
            {
                throw new Exception("ibs failed");
            }
        }

        public void SendPaymentNotification(double totalAmount, double meterAmount, double tipAmount, string authorizationCode,
            string vehicleNumber)
        {
        }

        public void SendMessageToDriver(string message, string vehicleNumber)
        {
        }

        public bool Fail { get; set; }
    }

    public class SinglePaymentServiceFactory : IPaymentServiceFactory
    {
        private readonly IPaymentService _instanceService;

        public SinglePaymentServiceFactory(IPaymentService instanceService)
        {
            _instanceService = instanceService;
        }

        public IPaymentService GetInstance()
        {
            return _instanceService;
        }
    }
}