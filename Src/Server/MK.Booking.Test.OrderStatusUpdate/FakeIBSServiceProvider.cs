using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.IBS.ChargeAccounts;
using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;

namespace MK.Booking.Test.OrderStatusUpdate
{
    public class FakeIBSServiceProvider : IIBSServiceProvider
    {

        public IBookingWebServiceClient Booking(string companyKey = null)
        {
            return new FakeBookingServiceClient();
        }

        #region

        public IAccountWebServiceClient Account(string companyKey = null)
        {
            throw new NotImplementedException();
        }

        public IStaticDataWebServiceClient StaticData(string companyKey = null)
        {
            throw new NotImplementedException();
        }

        public IBSSettingContainer GetSettingContainer(string companyKey = null)
        {
            throw new NotImplementedException();
        }

        public IChargeAccountWebServiceClient ChargeAccount(string companyKey = null)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class FakeBookingServiceClient : IBookingWebServiceClient
    {
        public IEnumerable<IBSOrderInformation> GetOrdersStatus(IList<int> ibsOrdersIds)
        {
            //Call to IBS
            Thread.Sleep(90);

            var response = ibsOrdersIds.Select(ibsOrdersId => new IBSOrderInformation
            {
                IBSOrderId =  ibsOrdersId,
                VehicleNumber = "12345",

            });

            return response;
        }
#region
        public int? CreateOrder(int? providerId, int accountId, string passengerName, string phone, int nbPassengers,
            int? vehicleTypeId, int? chargeTypeId, string note, DateTime pickupDateTime, IbsAddress pickup, IbsAddress dropoff,
            string accountNumber, int? customerNumber, string[] prompts, int?[] promptsLength, int defaultVehiculeTypeId,
            double? tipIncentive, Fare fare = null)
        {
            throw new NotImplementedException();
        }

        public IbsHailResponse Hail(Guid orderId, int? providerId, int accountId, string passengerName, string phone, int nbPassengers,
            int? vehicleTypeId, int? chargeTypeId, string note, DateTime pickupDateTime, IbsAddress pickup, IbsAddress dropoff,
            string accountNumber, int? customerNumber, string[] prompts, int?[] promptsLength, int defaultVehiculeTypeId,
            IEnumerable<IbsVehicleCandidate> vehicleCandidates, double? tipIncentive, Fare fare = null)
        {
            throw new NotImplementedException();
        }

        public IbsVehicleCandidate[] GetVehicleCandidates(IbsOrderKey orderKey)
        {
            throw new NotImplementedException();
        }

        public int? ConfirmHail(IbsOrderKey orderKey, IbsVehicleCandidate selectedVehicle)
        {
            throw new NotImplementedException();
        }

        public IbsOrderStatus GetOrderStatus(int orderId, int accountId, string contactPhone)
        {
            throw new NotImplementedException();
        }

        public IbsOrderDetails GetOrderDetails(int orderId, int accountId, string contactPhone)
        {
            throw new NotImplementedException();
        }

        public IbsFareEstimate GetFareEstimate(double? pickupLat, double? pickupLng, double? dropoffLat, double? dropoffLng,
            string pickupZipCode, string dropoffZipCode, string accountNumber, int? customerNumber, int? tripDurationInSeconds,
            int? providerId, int? vehicleTypeB, int defaultVehiculeTypeId)
        {
            throw new NotImplementedException();
        }

        public bool CancelOrder(int orderId, int accountId, string contactPhone)
        {
            throw new NotImplementedException();
        }

        public bool SendMessageToDriver(string message, string vehicleNumber)
        {
            throw new NotImplementedException();
        }

        public IbsVehiclePosition[] GetAvailableVehicles(double latitude, double longitude, int? vehicleTypeId)
        {
            throw new NotImplementedException();
        }

        public bool ConfirmExternalPayment(Guid orderId, int ibsOrderId, decimal totalAmount, decimal tipAmount, decimal meterAmount,
            string type, string provider, string transactionId, string authorizationCode, string cardToken, int accountID,
            string name, string phone, string email, string os, string userAgent)
        {
            throw new NotImplementedException();
        }

        public int? SendAccountInformation(Guid orderId, int ibsOrderId, string type, string cardToken, int accountID, string name,
            string phone, string email)
        {
            throw new NotImplementedException();
        }

        public bool UpdateOrderPaymentType(int ibsAccountId, int ibsOrderId, int? chargeTypeId)
        {
            throw new NotImplementedException();
        }

        public bool InitiateCallToDriver(int ibsOrderId, string vehicleNumber)
        {
            throw new NotImplementedException();
        }
#endregion
    }
}
