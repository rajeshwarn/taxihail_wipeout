using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Data;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Services
{
    public interface IIbsCreateOrderService
    {
        IBSOrderResult CreateIbsOrder(Guid orderId, Address pickupAddress, Address dropOffAddress, string accountNumberString, string customerNumberString,
            string companyKey, int ibsAccountId, string name, string phone, string email, int passengers, int? vehicleTypeId, string ibsInformationNote, bool isFutureBooking,
            DateTime pickupDate, string[] prompts, int?[] promptsLength, IList<ListItem> referenceDataCompanyList, string market, int? chargeTypeId,
            int? requestProviderId, Fare fare, double? tipIncentive, int? tipPercent, bool isHailRequest = false, int? companyFleedId = null);

        void CancelIbsOrder(int? ibsOrderId, string companyKey, string phone, Guid accountId);

        void UpdateOrderStatusAsync(Guid orderId);
    }
}
