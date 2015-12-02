using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Services
{
    public interface IDispatcherService
    {
        IBSOrderResult Dispatch(Guid accountId, Guid orderId, IbsOrderParams ibsOrderParams, BestAvailableCompany initialBestAvailableCompany,
            DispatcherSettingsResponse dispatcherSettings, string accountNumberString, int initialIbsAccountId, string name, string phone, int passengers,
            int? vehicleTypeId, string ibsInformationNote, DateTime pickupDate, string[] prompts, int?[] promptsLength, string market, Fare fare,
            double? tipIncentive, bool isHailRequest = false, List<string> driverIdsToExclude = null);

        void AssignJobToVehicle(string companyKey, IbsOrderKey ibsOrderKey, IbsVehicleCandidate ibsVehicleCandidate);

        DispatcherSettingsResponse GetSettings(string market, double? latitude = null, double? longitude = null, bool isHailRequest = false);

        DispatcherSettingsResponse GetSettings(double latitude, double longitude, bool isHailRequest = false);

        void CancelIbsOrder(int? ibsOrderId, string companyKey, string phone, int ibsAccountId);
    }
}
