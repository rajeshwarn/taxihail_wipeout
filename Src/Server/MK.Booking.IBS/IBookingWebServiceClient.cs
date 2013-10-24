using System;
using System.Collections.Generic;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.IBS
{
    public interface IBookingWebServiceClient 
    {        
        int? CreateOrder(int? providerId, int accountId, string passengerName, string phone, int nbPassengers, int? vehicleTypeId, int? chargeTypeId, string note, DateTime pickupDateTime, IBSAddress pickup, IBSAddress dropoff, Fare fare = default(Fare));

        IBSOrderStatus GetOrderStatus(int orderId, int accountId, string contactPhone);    

        IBSOrderDetails GetOrderDetails(int orderId, int accountId, string contactPhone);

        bool CancelOrder(int orderId, int accountId, string contactPhone);

        IEnumerable<IBSOrderInformation> GetOrdersStatus(IList<int> ibsOrdersIds);

        bool SendMessageToDriver(string message, string carId);

        IBSVehiclePosition[] GetAvailableVehicles(double latitude, double longitude);

        void ConfirmExternalPayment(int orderId, decimal amount,  string type, string provider, string transactionId, string authorizationCode);
    }
}