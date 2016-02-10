#region

using System;
using System.Collections.Generic;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.IBS
{
    public interface IBookingWebServiceClient
    {
        int? CreateOrder(int? providerId, int accountId, string passengerName, string phone, string email, int nbPassengers, int? vehicleTypeId, int? chargeTypeId, string note, DateTime pickupDateTime, IbsAddress pickup, IbsAddress dropoff, string accountNumber, int? customerNumber, string[] prompts, int?[] promptsLength, int defaultVehiculeTypeId, double? tipIncentive, int? tipPercent, Fare fare = default(Fare));

        IbsHailResponse Hail(Guid orderId, int? providerId, int accountId, string passengerName, string phone, string email, int nbPassengers, int? vehicleTypeId, int? chargeTypeId, string note, DateTime pickupDateTime, IbsAddress pickup, IbsAddress dropoff, string accountNumber, int? customerNumber, string[] prompts, int?[] promptsLength, int defaultVehiculeTypeId, IEnumerable<IbsVehicleCandidate> vehicleCandidates, double? tipIncentive, int? tipPercent, Fare fare = default(Fare));

        IbsVehicleCandidate[] GetVehicleCandidates(IbsOrderKey orderKey);

        int? ConfirmHail(IbsOrderKey orderKey, IbsVehicleCandidate selectedVehicle);

        IbsOrderStatus GetOrderStatus(int orderId, int accountId, string contactPhone);

        IbsOrderDetails GetOrderDetails(int orderId, int accountId, string contactPhone);

        IbsFareEstimate GetFareEstimate(double? pickupLat, double? pickupLng, double? dropoffLat, double? dropoffLng, string pickupZipCode, string dropoffZipCode, string accountNumber, int? customerNumber, int? tripDurationInSeconds, int? providerId, int? vehicleType, int defaultVehiculeTypeId);

        IBSDistanceEstimate GetDistanceEstimate(double distance, int? waitTime, int? stopCount, int? passengerCount, int? vehicleType, int defaultVehiculeTypeId, string accountNumber, int? customerNumber, int? tripTime);

        bool CancelOrder(int orderId, int accountId, string contactPhone);

        IEnumerable<IBSOrderInformation> GetOrdersStatus(IList<int> ibsOrdersIds);

        bool SendMessageToDriver(string message, string vehicleNumber);

        IbsVehiclePosition[] GetAvailableVehicles(double latitude, double longitude, int? vehicleTypeId);

        bool ConfirmExternalPayment(Guid orderId, int ibsOrderId, decimal totalAmount, decimal tipAmount, decimal meterAmount, string type, string provider, string transactionId,
            string authorizationCode, string cardToken, int accountID, string name, string phone, string email, string os, string userAgent, decimal fareAmount = 0, decimal extrasAmount = 0, 
            decimal vatAmount = 0, decimal discountAmount = 0, decimal tollAmount = 0, decimal surchargeAmount = 0);
 
        int? SendAccountInformation(Guid orderId, int ibsOrderId, string type, string cardToken, int accountID, string name, string phone, string email);

        bool UpdateOrderPaymentType(int ibsAccountId, int ibsOrderId, int? chargeTypeId);

        bool InitiateCallToDriver(int ibsOrderId, string vehicleNumber);

        bool UpdateDropOffInTrip(int ibsOrderId, Guid orderId, Address dropOffAddress);
    }
}