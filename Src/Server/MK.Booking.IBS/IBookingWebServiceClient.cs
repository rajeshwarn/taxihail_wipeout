#region

using System;
using System.Collections.Generic;
using apcurium.MK.Common;

#endregion

namespace apcurium.MK.Booking.IBS
{
    public interface IBookingWebServiceClient
    {
        int? CreateOrder(int? providerId, int accountId, string passengerName, string phone, int nbPassengers,
            int? vehicleTypeId, int? chargeTypeId, string note, DateTime pickupDateTime, IbsAddress pickup,
            IbsAddress dropoff, Fare fare = default(Fare));

        IbsOrderStatus GetOrderStatus(int orderId, int accountId, string contactPhone);

        IbsOrderDetails GetOrderDetails(int orderId, int accountId, string contactPhone);

        IbsFareEstimate GetFareEstimate(double? pickupLat, double? pickupLng, double? dropoffLat, double? dropoffLng);

        bool CancelOrder(int orderId, int accountId, string contactPhone);

        IEnumerable<IBSOrderInformation> GetOrdersStatus(IList<int> ibsOrdersIds);

        bool SendMessageToDriver(string message, string carId);

        IbsVehiclePosition[] GetAvailableVehicles(double latitude, double longitude);

        void ConfirmExternalPayment(int orderId, decimal amount, string type, string provider, string transactionId,
            string authorizationCode);
    }
}