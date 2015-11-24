#region
using apcurium.MK.Booking.Api.Client.Extensions;

using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class IbsFareServiceClient : BaseServiceClient, IIbsFareClient
    {
        public IbsFareServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }


        public Task<DirectionInfo> GetDirectionInfoFromIbs(double pickupLatitude, double pickupLongitude,
            double dropoffLatitude, double dropoffLongitude, 
			string pickupZipCode, string dropoffZipCode,
			string accountNumber, int? tripDurationInSeconds,
			int? vehicleType)
        {
            var req = string.Format(CultureInfo.InvariantCulture,
                "/ibsfare?PickupLatitude={0}&PickupLongitude={1}&DropoffLatitude={2}&DropoffLongitude={3}&" +
				"PickupZipCode={4}&DropoffZipCode={5}&" +
				"AccountNumber={6}&CustomerNumber={7}&TripDurationInSeconds={8}&vehicleType={9}",
                pickupLatitude, pickupLongitude, dropoffLatitude, dropoffLongitude, 
				pickupZipCode, dropoffZipCode,
				accountNumber.ToSafeString(), 0, tripDurationInSeconds.HasValue ? tripDurationInSeconds.ToString() : "", vehicleType);
            var result = Client.GetAsync<DirectionInfo>(req);
            return result;
        }

        public Task<DirectionInfo> GetDirectionInfoFromDistance(double? distance, int? waitTime, 
            int? stopCount, int? passengerCount, 
            int? vehicleType, int defaultVehiculeTypeId, 
            string accountNumber, int? customerNumber, 
            int? tripTime)
        {
            waitTime = waitTime.HasValue && waitTime > 0 ? waitTime.Value : 0;
            distance = distance.HasValue && distance > 0 ? distance.Value : 0;
            stopCount = stopCount.HasValue && stopCount > 0 ? stopCount.Value : 0;
            passengerCount = passengerCount.HasValue && passengerCount > 0 ? passengerCount.Value : 0;
            vehicleType = vehicleType ?? defaultVehiculeTypeId;
            tripTime = tripTime.HasValue && tripTime > 0 ? tripTime.Value : 0;

            if (accountNumber.HasValue() && customerNumber.HasValue)
            {
                customerNumber = customerNumber.Value;
            }
            else
            {
                customerNumber = -1;
            }

            var req = string.Format(CultureInfo.InvariantCulture,
                "/ibsdistance?Distance={0}&WaitTime={1}&StopCount={2}&PassengerCount={3}&AccountNumber={4}&CustomerNumber={5}&VehicleType={6}&TripTime={7}",
                distance.Value, waitTime.Value, stopCount.Value, passengerCount.Value, 
                accountNumber, customerNumber.Value,
                vehicleType.Value, tripTime.Value);
            var result = Client.GetAsync<DirectionInfo>(req);
            return result;
        }
    }
}