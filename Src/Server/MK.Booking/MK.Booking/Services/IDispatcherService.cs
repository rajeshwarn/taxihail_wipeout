using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Services
{
    public interface IDispatcherService
    {
        IBSOrderResult Dispatch(Guid accountId, Guid orderId, BestAvailableCompany initialBestAvailableCompany, DispatcherSettingsResponse dispatcherSettings,
            IbsAddress pickupAddress, IbsAddress dropOffAddress, string accountNumberString, int? customerNumber,
            int initialIbsAccountId, string name, string phone, int passengers, int vehicleTypeId, string ibsInformationNote,
            DateTime pickupDate, string[] prompts, int?[] promptsLength, IList<ListItem> initialReferenceDataCompanyList, string market, int? chargeTypeId,
            int? initialProviderId, int? homeMarketProviderId, Fare fare, double? tipIncentive, bool isHailRequest = false);

        void AssignJobToVehicle(string companyKey, IbsOrderKey ibsOrderKey, IbsVehicleCandidate ibsVehicleCandidate);

        IEnumerable<VehicleCandidate> WaitForCandidatesResponse(string companyKey, IbsOrderKey ibsOrderKey, DispatcherSettingsResponse dispatcherSettings);

        DispatcherSettingsResponse GetSettings(string market, double? latitude = null, double? longitude = null, bool isHailRequest = false);

        DispatcherSettingsResponse GetSettings(double latitude, double longitude, bool isHailRequest = false);

        IEnumerable<VehicleCandidate> GetVehicleCandidates(Guid orderId, BestAvailableCompany bestAvailableCompany, DispatcherSettingsResponse dispatcherSettings, double pickupLatitude, double pickupLongitude);

        Dictionary<Guid, List<Tuple<string, string>>> GetLegacyVehicleIdMapping();

        void CancelIbsOrder(int? ibsOrderId, string companyKey, string phone, int ibsAccountId);
    }
}
