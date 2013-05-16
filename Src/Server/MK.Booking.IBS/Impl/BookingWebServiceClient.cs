using System;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using ServiceStack.Text;
using apcurium.MK.Common;
using System.Threading;

namespace apcurium.MK.Booking.IBS.Impl
{
    public class BookingWebServiceClient : BaseService<WebOrder7Service>, IBookingWebServiceClient
    {

        private const int _invalidZoneErrorCode = -1002;
        private IStaticDataWebServiceClient _staticDataWebServiceClient;
        private IDriverWebServiceClient _driverService;

        public BookingWebServiceClient(IConfigurationManager configManager, ILogger logger, IStaticDataWebServiceClient staticDataWebServiceClient, IDriverWebServiceClient driverService)
            : base(configManager, logger)
        {
            _staticDataWebServiceClient = staticDataWebServiceClient;
            _driverService = driverService;

        }

        protected override string GetUrl()
        {
            return base.GetUrl() + "IWEBOrder_7";
        }
        
        public IBSOrderStatus GetOrderStatus(int orderId, int accountId, string contactPhone)
        {
            var status = new IBSOrderStatus { Status = TWEBOrderStatusValue.wosNone.ToString() };
            UseService(service =>
            {
                var orderStatus = service.GetOrderStatus(UserNameApp, PasswordApp, orderId, contactPhone, string.Empty, accountId);
                status.Status = orderStatus.ToString();

                double latitude = 0;
                double longitude = 0;
                var result = service.GetVehicleLocation(UserNameApp, PasswordApp, orderId, ref longitude, ref latitude);

                if (result == 0)
                {
                    status.VehicleLatitude = latitude;
                    status.VehicleLongitude = longitude;
                }

            });
            return status;
        }

        public IBSOrderDetails GetOrderDetails(int orderId, int accountId, string contactPhone)
        {
            var result = new IBSOrderDetails();
            UseService(service =>
            {
                var order = service.GetBookOrder_7(UserNameApp, PasswordApp, orderId, contactPhone, null, accountId);
                if (order != null)
                {
                    result.VehicleNumber = order.CabNo.ToSafeString().Trim();
                    result.Fare = order.Fare == 0 ? (double?)null : order.Fare;
                    result.Toll = order.Tolls == 0 ? (double?)null : order.Tolls;
                    result.Tip = order.Tips == 0 ? (double?)null : order.Tips; //TODO à enlever
                    result.CallNumber = order.CallNumber;
                }
            });
            return result;
        }


        public int? CreateOrder(int? providerId, int accountId, string passengerName, string phone, int nbPassengers, int? vehicleTypeId, int? chargeTypeId, string note, DateTime pickupDateTime, IBSAddress pickup, IBSAddress dropoff)
        {
            Logger.LogMessage("WebService Create Order call : accountID=" + accountId);
            var order = new TBookOrder_7();

            order.ServiceProviderID = providerId;
            order.AccountID = accountId;
            order.Customer = passengerName;
            order.Phone = phone;

            var autoDispatch = ConfigManager.GetSetting("IBS.AutoDispatch").SelectOrDefault( setting => bool.Parse( setting ) , true );
            order.DispByAuto = autoDispatch;

            var priority = ConfigManager.GetSetting("IBS.OrderPriority").SelectOrDefault(setting => bool.Parse(setting), true);
            order.Priority = priority ? 1 : 0;
                       

            order.PickupDate = new TWEBTimeStamp { Year = pickupDateTime.Year, Month = pickupDateTime.Month, Day = pickupDateTime.Day };
            order.PickupTime = new TWEBTimeStamp { Hour = pickupDateTime.Hour, Minute = pickupDateTime.Minute, Second = 0, Fractions = 0 };

            order.ChargeTypeID = chargeTypeId ?? 0;
            var aptRing = Params.Get(pickup.Apartment, pickup.RingCode).Where(s => s.HasValue()).JoinBy(" / ");

            order.PickupAddress = new TWEBAddress { StreetPlace = pickup.FullAddress, AptBaz = aptRing, Longitude = pickup.Longitude, Latitude = pickup.Latitude, Postal = pickup.ZipCode };
            order.DropoffAddress = dropoff == null ? new TWEBAddress() : new TWEBAddress { StreetPlace = dropoff.FullAddress, AptBaz = dropoff.Apartment, Longitude = dropoff.Longitude, Latitude = dropoff.Latitude, Postal = dropoff.ZipCode };
            order.Passengers = nbPassengers;
            order.VehicleTypeID = vehicleTypeId ?? 0;
            order.Note = note;
            order.ContactPhone = phone;
            order.OrderStatus = TWEBOrderStatusValue.wosPost;


            int? orderId = null;

            if ( !ValidateZoneAddresses(order))
            {
                return _invalidZoneErrorCode;
            }

            UseService(service =>
            {
                Logger.LogMessage("WebService Creating IBS Order : " +  JsonSerializer.SerializeToString( order, typeof( TBookOrder_7 ) ) );
                Logger.LogMessage("WebService Creating IBS Order pickup : " + JsonSerializer.SerializeToString(order.PickupAddress, typeof(TWEBAddress)));
                Logger.LogMessage("WebService Creating IBS Order dest : " + JsonSerializer.SerializeToString(order.DropoffAddress, typeof(TWEBAddress)));


                orderId = service.SaveBookOrder_7(UserNameApp, PasswordApp, order);                
                Logger.LogMessage("WebService Create Order, orderid receveid : " + orderId);
            });
            return orderId;
        }

        private bool ValidateZoneAddresses(TBookOrder_7 order)
        {
            if (!ValidateZone(order.PickupAddress, "IBS.ValidatePickupZone", "IBS.PickupZoneToExclude"))
            {
                return false;
            }

            if ((order.DropoffAddress != null) && (order.DropoffAddress.Latitude != 0) && (order.DropoffAddress.Latitude != 0))
            {
                if (!ValidateZone(order.DropoffAddress, "IBS.ValidateDestinationZone", "IBS.DestinationZoneToExclude"))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidateZone(TWEBAddress tWEBAddress, string enableValidationKey, string excludedZoneKey)
        {
            var isValidationEnabled = bool.Parse(ConfigManager.GetSetting(enableValidationKey));
            if (isValidationEnabled)
            {
                var zone = _staticDataWebServiceClient.GetZoneByCoordinate(tWEBAddress.Latitude, tWEBAddress.Longitude);
                if ( zone.ToSafeString().Trim().IsNullOrEmpty())
                {
                    return false;
                }

                var excludedZones = ConfigManager.GetSetting(excludedZoneKey).Split(',');
                if (excludedZones.Any() && excludedZones.Any(z => z.SoftEqual(zone)))
                {
                    return false;
                }

            }
            return true;

        }

        public bool CancelOrder(int orderId, int accountId, string contactPhone)
        {
            Logger.LogMessage("WebService Cancel Order call : " + orderId + " " + accountId);
            bool isCompleted = false;
            UseService(service =>
            {
                int count = 0;
                IBSOrderStatus status = null;
                
                //We need to try 5 times because sometime the IBS cancel method doesn't return an error but doesn't cancel the ride... after 5 time, we are giving up.
                do
                {
                    if (count > 0)
                    {
                        Logger.LogMessage("WebService Cancel Order is not working!  Trying again in 500ms  : " + orderId + " " + accountId);
                        Thread.Sleep(500);
                    }

                    var result = service.CancelBookOrder(UserNameApp, PasswordApp, orderId, contactPhone, null, accountId);
                    count++;
                    isCompleted = result == 0;
                    status = GetOrderStatus(orderId, accountId, contactPhone);
                }
                while ( (status.Status.ToSafeString().Contains( "Cancel")) && ( count <= 5 ) );
                
            });
            return isCompleted;
        }
    }
}