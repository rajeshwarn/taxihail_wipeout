using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Data;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Services
{
    public interface IIbsOrderService
    {
        IBSOrderResult CreateIbsOrder(Guid orderId, Address pickupAddress, Address dropOffAddress, string accountNumberString, string customerNumberString, string companyKey,
            int ibsAccountId, string name, string phone, int passengers, int? vehicleTypeId, string ibsInformationNote,
            DateTime pickupDate, string[] prompts, int?[] promptsLength, IList<ListItem> referenceDataCompanyList, string market, int? chargeTypeId,
            int? requestProviderId, Fare fare, double? tipIncentive, bool isHailRequest = false);

        void CancelIbsOrder(int? ibsOrderId, string companyKey, string phone, Guid accountId);

        void ConfirmExternalPayment(Guid orderId, int ibsOrderId, decimal totalAmount, decimal tipAmount, decimal meterAmount, string type, string provider, string transactionId,
        
        string authorizationCode, string cardToken, int accountId, string name, string phone, string email, string os, string userAgent, string companyKey);

        void SendPaymentNotification(double totalAmount, double meterAmount, double tipAmount, string authorizationCode, string vehicleNumber, string companyKey = null);

        void SendMessageToDriver(string message, string vehicleNumber, string companyKey);

        void UpdateOrderStatusAsync(Guid orderId);
    }
}
