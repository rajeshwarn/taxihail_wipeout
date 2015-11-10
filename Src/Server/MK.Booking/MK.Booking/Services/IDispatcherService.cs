using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Services
{
    public interface IDispatcherService
    {
        void Dispatch(BestAvailableCompany bestAvailableCompany, DispatcherSettingsResponse dispatcherSettings);

        void AssignJobToVehicle(string companyKey, IbsOrderKey ibsOrderKey, IbsVehicleCandidate ibsVehicleCandidate);

        IEnumerable<VehicleCandidate> WaitForCandidatesResponse(string companyKey, IbsOrderKey ibsOrderKey, DispatcherSettingsResponse dispatcherSettings);

        DispatcherSettingsResponse GetSettings(string market, double? latitude = null, double? longitude = null, bool isHailRequest = false);

        DispatcherSettingsResponse GetSettings(double latitude, double longitude, bool isHailRequest = false);

        IEnumerable<VehicleCandidate> GetVehicleCandidates(Guid orderId, BestAvailableCompany bestAvailableCompany, DispatcherSettingsResponse dispatcherSettings, double pickupLatitude, double pickupLongitude);

        Dictionary<Guid, Tuple<string, string>> GetLegacyVehicleIdMapping();
    }
}
