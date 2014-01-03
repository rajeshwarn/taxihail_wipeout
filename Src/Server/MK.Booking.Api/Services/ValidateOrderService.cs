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

        public ValidateOrderService(
            IConfigurationManager configManager,
            IStaticDataWebServiceClient staticDataWebServiceClient,
            IRuleCalculator ruleCalculator)
        {
            _configManager = configManager;
            _staticDataWebServiceClient = staticDataWebServiceClient;
            _ruleCalculator = ruleCalculator;
        }

        public object Post(ValidateOrderRequest request)
        {
            Log.Info("Validating order request : ");


            var rule = _ruleCalculator.GetActiveWarningFor(request.PickupDate.HasValue,
                request.PickupDate.HasValue ? request.PickupDate.Value : GetCurrentOffsetedTime(),
                () =>
                {
                    var zone = request.TestZone;
                    if (!request.TestZone.HasValue())
                    {
                        zone = _staticDataWebServiceClient.GetZoneByCoordinate(request.Settings.ProviderId,
                            request.PickupAddress.Latitude, request.PickupAddress.Longitude);
                    }
                    return zone;
                });

            return new OrderValidationResult {HasWarning = rule != null, Message = rule != null ? rule.Message : null};
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