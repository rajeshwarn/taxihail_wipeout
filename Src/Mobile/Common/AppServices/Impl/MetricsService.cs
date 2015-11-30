using System;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Cirrious.CrossCore;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class MetricsService : BaseService, IMetricsService
    {
        private readonly ILocationService _locationService;

        public MetricsService(ILocationService locationService)
        {
            _locationService = locationService;
        }

        public async void LogApplicationStartUp()
        {
            try
            {
                var packageInfo = Mvx.Resolve<IPackageInfo>();

                var position = await _locationService.GetUserPosition();

                var request = new LogApplicationStartUpRequest
                {
                    StartUpDate = DateTime.UtcNow,
                    Platform = packageInfo.Platform,
                    PlatformDetails = packageInfo.PlatformDetails,
                    ApplicationVersion = packageInfo.Version,
                    Latitude = position != null
                        ? position.Latitude
                        : 0,
                    Longitude = position != null
                        ? position.Longitude
                        : 0
                };

                //This needs to be awaited to catch exceptions and must be the last task to be awaited before the end of this try catch block.
                await UseServiceClientAsync<MetricsServiceClient>(client => client.LogApplicationStartUp(request));
            }
            catch (Exception ex)
            {
                // If logging fails, run app anyway and log exception
                Logger.LogError(ex);
            }
        }

        public async void LogOriginalRideEta(Guid orderId, long? originalEta)
        {
            try
            {
                var request = new LogOriginalEtaRequest
                {
                    OrderId = orderId,
                    OriginalEta = originalEta
                };

                //This needs to be awaited to catch exceptions and must be the last task to be awaited before the end of this try catch block.
                await UseServiceClientAsync<MetricsServiceClient>(client => client.LogOriginalRideEta(request));
            }
            catch (Exception ex)
            {
                // If logging fails, run app anyway and log exception
                Logger.LogError(ex);
            }
        }
    }
}