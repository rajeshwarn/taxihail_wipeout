using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Common.Diagnostic;
using CMTPayment.Pair;

namespace CMTPayment
{
    public class CmtTripInfoServiceHelper
    {
        private readonly CmtMobileServiceClient _cmtMobileServiceClient;
        private readonly ILogger _logger;

        public CmtTripInfoServiceHelper(CmtMobileServiceClient cmtMobileServiceClient, ILogger logger)
        {
            _cmtMobileServiceClient = cmtMobileServiceClient;
            _logger = logger;
        }


        public Trip GetTripInfo(string pairingToken)
        {
            try
            {
                var trip = _cmtMobileServiceClient.Get(new TripRequest { Token = pairingToken });
                if (trip != null)
                {
                    //ugly fix for deserilization problem in datetime on the device for IOS
                    if (trip.StartTime.HasValue)
                    {
                        trip.StartTime = DateTime.SpecifyKind(trip.StartTime.Value, DateTimeKind.Local);
                    }

                    if (trip.EndTime.HasValue)
                    {
                        trip.EndTime = DateTime.SpecifyKind(trip.EndTime.Value, DateTimeKind.Local);
                    }
                }

                return trip;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Trip WaitForTripInfo(string pairingToken, long timeoutSeconds)
        {
            // wait for trip to be updated
            var watch = new Stopwatch();
            watch.Start();
            var trip = GetTripInfo(pairingToken);
            while (trip == null)
            {
                Thread.Sleep(2000);
                trip = GetTripInfo(pairingToken);

                if (watch.Elapsed.TotalSeconds >= timeoutSeconds)
                {
                    _logger.LogMessage("Timeout Exception, Could not be paired with vehicle.");
                    throw new TimeoutException("Could not be paired with vehicle");
                }
            }

            return trip;
        }

        public void WaitForRideLinqUnpaired(string pairingToken, long timeoutSeconds)
        {
            var watch = new Stopwatch();
            watch.Start();
            var trip = GetTripInfo(pairingToken);
            while (trip != null)
            {
                Thread.Sleep(2000);
                trip = GetTripInfo(pairingToken);

                if (watch.Elapsed.TotalSeconds >= timeoutSeconds)
                {
                    throw new TimeoutException("Could not be unpaired of vehicle");
                }
            }
        }
    }
}
