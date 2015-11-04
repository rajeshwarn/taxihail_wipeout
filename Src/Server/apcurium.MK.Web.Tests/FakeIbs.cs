using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Web.Tests
{
    public class FakeIbs : IIbsOrderService
    {
        public IBSOrderResult CreateIbsOrder(Guid orderId, Address pickupAddress, Address dropOffAddress, string accountNumberString,
            string customerNumberString, string companyKey, int ibsAccountId, string name, string phone, int passengers,
            int? vehicleTypeId, string ibsInformationNote, DateTime pickupDate, string[] prompts, int?[] promptsLength,
            IList<ListItem> referenceDataCompanyList, string market, int? chargeTypeId, int? requestProviderId, Fare fare,
            double? tipIncentive, bool isHailRequest = false)
        {
            throw new NotImplementedException();
        }

        public void CancelIbsOrder(int? ibsOrderId, string companyKey, string phone, Guid accountId)
        {
            throw new NotImplementedException();
        }

        public void ConfirmExternalPayment( Guid orderId, int ibsOrderId, decimal totalAmount, decimal tipAmount, decimal meterAmount, string type,
            string provider, string transactionId, string authorizationCode, string cardToken, int accountId, string name,
            string phone, string email, string os, string userAgent, ServiceType serviceType, string companyKey = null)
        {
            if (Fail)
            {
                throw new Exception("ibs failed");
            }
        }

        public void SendPaymentNotification(double totalAmount, double taxedMeterAmount, double tipAmount, string authorizationCode,
            string vehicleNumber, ServiceType serviceType, string companyKey = null)
        {
        }

        public void SendMessageToDriver(string message, string vehicleNumber, ServiceType serviceType, string companyKey = null)
        {
        }

        public void UpdateOrderStatusAsync(Guid orderId)
        {
            throw new NotImplementedException();
        }

        public bool Fail { get; set; }
    }
}