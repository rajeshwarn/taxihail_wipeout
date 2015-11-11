using System;
using System.Diagnostics;
using System.Threading;
using apcurium.MK.Common.Diagnostic;
using CMTPayment.Pair;
using ServiceStack.ServiceClient.Web;
using ServiceStack.Text;
using System.Net;

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
                var trip = _cmtMobileServiceClient.Get(new TripRequest {Token = pairingToken});
                if (trip == null)
                {
                    _logger.LogMessage("No Trip info found for pairing token {0}", pairingToken);
                    return null;
                }

                // Ugly fix for deserilization problem in datetime on the device for IOS
                if (trip.StartTime.HasValue)
                {
                    trip.StartTime = DateTime.SpecifyKind(trip.StartTime.Value, DateTimeKind.Local);
                }

                if (trip.EndTime.HasValue)
                {
                    trip.EndTime = DateTime.SpecifyKind(trip.EndTime.Value, DateTimeKind.Local);
                }

                _logger.LogMessage("Following trip info found from pairing token {0} \n\r {1}", pairingToken, trip.ToJson());

				trip.HttpStatusCode = (int)HttpStatusCode.OK;

                return trip;
            }
            catch (WebServiceException ex)
            {
                _logger.LogMessage("An WebService error with error code {0} and status code {1}  occured while trying to get the CMT trip info for Pairing Token: {2}",
                    ex.ErrorCode ?? "Unknown",
                    ex.StatusCode,
                    pairingToken);

                if (ex.ResponseBody != null)
                {
                    _logger.LogMessage("Error Response: {0}", ex.ResponseBody);

                    var errorResponse = ex.ResponseBody.FromJson<ErrorResponse>();
                    return new Trip { HttpStatusCode = ex.StatusCode, ErrorCode = errorResponse.ResponseCode };
                }

                _logger.LogError(ex);
                _logger.LogStack();

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogMessage("An error occured while trying to get the CMT trip info for Pairing Token: {0}", pairingToken);
                _logger.LogError(ex);
                _logger.LogStack();

                return null;
            }
        }

        public Trip WaitForTripInfo(string pairingToken, long timeoutSeconds)
        {
            // wait for trip to be updated
            var watch = new Stopwatch();
            watch.Start();
            var trip = GetTripInfo(pairingToken);

            while (trip == null || trip.ErrorCode.HasValue)
            {
				if (trip.HttpStatusCode == (int)HttpStatusCode.BadRequest && (trip.ErrorCode == CmtErrorCodes.UnableToPair || trip.ErrorCode == CmtErrorCodes.CreditCardDeclinedOnPreauthorization || trip.ErrorCode == CmtErrorCodes.UnablePreauthorizeCreditCard))
				{
					return trip;
				}

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

            while (trip != null && !trip.ErrorCode.HasValue)
            {
                Thread.Sleep(2000);
                trip = GetTripInfo(pairingToken);

                if (watch.Elapsed.TotalSeconds >= timeoutSeconds)
                {
                    throw new TimeoutException("Could not be unpaired of vehicle");
                }
            }
        }

        public Trip CheckForTripEndErrors(string pairingToken)
        {
            var timeToWaitForErrors = 45; // In seconds

            var watch = new Stopwatch();
            watch.Start();
            var trip = GetTripInfo(pairingToken);

            while (trip != null && !trip.ErrorCode.HasValue)
            {
                Thread.Sleep(2000);
                trip = GetTripInfo(pairingToken);

                if (watch.Elapsed.TotalSeconds >= timeToWaitForErrors)
                {
                    // No errors found
                    return trip;
                }
            }

            // Errors found
            return trip;
        }

        public void WaitForTipUpdated(string pairingToken, int updatedTipPercentage, long timeoutSeconds)
        {
            var watch = new Stopwatch();
            watch.Start();
            var trip = GetTripInfo(pairingToken);

            while (trip.AutoTipPercentage != updatedTipPercentage)
            {
                Thread.Sleep(2000);
                trip = GetTripInfo(pairingToken);

                if (watch.Elapsed.TotalSeconds >= timeoutSeconds)
                {
                    throw new TimeoutException("Could not update tip");
                }
            }
        }
    }
}
