#region

using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Calculator;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using log4net;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ValidateOrderService : Service
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (ValidateOrderService));

        private readonly IConfigurationManager _configManager;
        private readonly IRuleCalculator _ruleCalculator;
        private readonly IStaticDataWebServiceClient _staticDataWebServiceClient;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;

        public ValidateOrderService(
            IConfigurationManager configManager,
            IStaticDataWebServiceClient staticDataWebServiceClient,
            IBookingWebServiceClient bookingWebServiceClient,
            IRuleCalculator ruleCalculator)
        {
            _configManager = configManager;
            _staticDataWebServiceClient = staticDataWebServiceClient;
            _bookingWebServiceClient = bookingWebServiceClient;
            _ruleCalculator = ruleCalculator;
        }

        public object Post(ValidateOrderRequest request)
        {
            Log.Info("Validating order request : ");

            var pickupZone = request.TestZone;
            if (!request.TestZone.HasValue())
            {
                pickupZone = _staticDataWebServiceClient.GetZoneByCoordinate(request.Settings.ProviderId,
                    request.PickupAddress.Latitude, request.PickupAddress.Longitude);
            }

            string dropoffZone = null;
            if (request.DropOffAddress != null)
            {
                dropoffZone = _staticDataWebServiceClient.GetZoneByCoordinate(request.Settings.ProviderId,
                    request.DropOffAddress.Latitude, request.DropOffAddress.Longitude);
            }
                       

            if (request.ForError)
            {
                //pass dropoff because aexid is using it only for dropoff
                var rule = _ruleCalculator.GetActiveDisableFor(request.PickupDate.HasValue,
                   request.PickupDate.HasValue ? request.PickupDate.Value : GetCurrentOffsetedTime(),
                   () => dropoffZone);

                //if the rule for disable has passed then we can check the exclusion zone
                var hasError = rule != null;
                var message = rule != null ? rule.Message : null;

                if(!hasError)
                {
                    var invalidPickUpZone = _bookingWebServiceClient.ValidateZone(pickupZone, "IBS.ValidatePickupZone",
                        "IBS.PickupZoneToExclude");
                    var invalidDropoffZone = _bookingWebServiceClient.ValidateZone(dropoffZone, "IBS.ValidateDestinationZone",
                        "IBS.DestinationZoneToExclude");

                    hasError = invalidPickUpZone || invalidDropoffZone;
                    if (hasError)
                    {
                        message = invalidPickUpZone ? "Cette zone de départ n'est pas desservie" : "Cette zone d'arrivée n'est pas desservie";
                    }
                }

                return new OrderValidationResult
                {
                    HasError = hasError,
                    Message = message
                };
            }
            else
            {

                var rule = _ruleCalculator.GetActiveWarningFor(request.PickupDate.HasValue,
                    request.PickupDate.HasValue ? request.PickupDate.Value : GetCurrentOffsetedTime(),
                    () => pickupZone);

                return new OrderValidationResult
                {
                    HasWarning = rule != null,
                    Message = rule != null ? rule.Message : null
                };
            }
    
        }

        private DateTime GetCurrentOffsetedTime()
        {
            var ibsServerTimeDifference =
                _configManager.GetSetting("IBS.TimeDifference").SelectOrDefault(setting => long.Parse(setting), 0);
            var offsetedTime = DateTime.Now.AddMinutes(2);
            if (ibsServerTimeDifference != 0)
            {
                offsetedTime = offsetedTime.Add(new TimeSpan(ibsServerTimeDifference));
            }

            return offsetedTime;
        }
    }
}