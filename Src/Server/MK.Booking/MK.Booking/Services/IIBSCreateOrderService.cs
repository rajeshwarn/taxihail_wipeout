﻿using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Data;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Services
{
    public interface IIbsCreateOrderService
    {
        IBSOrderResult CreateIbsOrder(Guid orderId, Address pickupAddress, Address dropOffAddress, string accountNumberString, string customerNumberString,
            ServiceType serviceType, string companyKey, int ibsAccountId, string name, string phone, int passengers, int? vehicleTypeId, string ibsInformationNote,
            DateTime pickupDate, string[] prompts, int?[] promptsLength, IList<ListItem> referenceDataCompanyList, string market, int? chargeTypeId,
            int? requestProviderId, Fare fare, double? tipIncentive, string email, int? tipPercent, bool isHailRequest = false, int? companyFleedId = null);

        void CancelIbsOrder(int? ibsOrderId, string companyKey, ServiceType serviceType, string phone, Guid accountId);

        void UpdateOrderStatusAsync(Guid orderId);
    }
}
