#region

using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Calculator;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Client.Impl;
using log4net;
using apcurium.MK.Booking.Maps.Geo;
using CustomerPortal.Contract.Response;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ValidateOrderService : BaseApiService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (ValidateOrderService));

        private readonly IServerSettings _serverSettings;
        private readonly IRuleCalculator _ruleCalculator;
        private readonly TaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;
        private readonly IIBSServiceProvider _ibsServiceProvider;

        public ValidateOrderService(
            IServerSettings serverSettings,
            IIBSServiceProvider ibsServiceProvider,
            IRuleCalculator ruleCalculator,
            TaxiHailNetworkServiceClient taxiHailNetworkServiceClient)
        {
            _serverSettings = serverSettings;
            _ibsServiceProvider = ibsServiceProvider;
            _ruleCalculator = ruleCalculator;
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;
        }

        public async Task<OrderValidationResult> Post(ValidateOrderRequest request)
        {
            if (_serverSettings.ServerData.IBS.FakeOrderStatusUpdate)
            {
                request.TestZone = "101";
            }

            Func<string> getPickupZone = () => request.TestZone.HasValue() 
                ? request.TestZone 
                : _ibsServiceProvider.StaticData().GetZoneByCoordinate(request.Settings.ProviderId, request.PickupAddress.Latitude, request.PickupAddress.Longitude);

            Func<string> getDropoffZone = () => request.DropOffAddress != null
                ? _ibsServiceProvider.StaticData().GetZoneByCoordinate(request.Settings.ProviderId, request.DropOffAddress.Latitude, request.DropOffAddress.Longitude)
                : null;

            CompanyMarketSettingsResponse marketSettings;

            try
            {
                // Find market
                marketSettings = await _taxiHailNetworkServiceClient.GetCompanyMarketSettings(request.PickupAddress.Latitude, request.PickupAddress.Longitude);
            }
            catch (Exception ex)
            {
                Log.Info("Unable to fetch market");
                Log.Error(ex);

                return new OrderValidationResult()
                {
                    HasError = true,
                    Message = "Unable to fetch market"
                };

            }

            if (request.ForError)
            {
                var rule = _ruleCalculator.GetActiveDisableFor(request.PickupDate.HasValue,
                                        request.PickupDate ?? GetCurrentOffsetedTime(),
                                        getPickupZone, getDropoffZone, marketSettings.Market, new Position(request.PickupAddress.Latitude, request.PickupAddress.Longitude));

                var hasError = rule != null;
                var message = rule != null ? rule.Message : null;
                var disableFutureBooking = !marketSettings.EnableFutureBooking || _ruleCalculator.GetDisableFutureBookingRule(marketSettings.Market) != null;

                Log.Debug(string.Format("Has Error : {0}, Message: {1}", hasError, message));

                return new OrderValidationResult
                {
                    HasError = hasError,
                    Message = message,
                    AppliesToCurrentBooking = rule != null && rule.AppliesToCurrentBooking,
                    AppliesToFutureBooking = rule != null && rule.AppliesToFutureBooking,
                    DisableFutureBooking = disableFutureBooking
                };
            }
            else
            {
                var rule = _ruleCalculator.GetActiveWarningFor(request.PickupDate.HasValue,
                                        request.PickupDate ?? GetCurrentOffsetedTime(),
										getPickupZone, getDropoffZone, marketSettings.Market, new Position(request.PickupAddress.Latitude, request.PickupAddress.Longitude));

                var hasWarning = rule != null;
                var message = rule != null ? rule.Message : null;
                var disableFutureBooking = !marketSettings.EnableFutureBooking || _ruleCalculator.GetDisableFutureBookingRule(marketSettings.Market) != null;

                return new OrderValidationResult
                {
                    HasWarning = hasWarning,
                    Message = message,
                    AppliesToCurrentBooking = rule != null && rule.AppliesToCurrentBooking,
                    AppliesToFutureBooking = rule != null && rule.AppliesToFutureBooking,
                    DisableFutureBooking = disableFutureBooking
                };
            }
        }

        private DateTime GetCurrentOffsetedTime()
        {
            var ibsServerTimeDifference = _serverSettings.ServerData.IBS.TimeDifference;
            var offsetedTime = DateTime.Now.AddMinutes(2);
            if (ibsServerTimeDifference != 0)
            {
                offsetedTime = offsetedTime.Add(new TimeSpan(ibsServerTimeDifference));
            }

            return offsetedTime;
        }
    }
}