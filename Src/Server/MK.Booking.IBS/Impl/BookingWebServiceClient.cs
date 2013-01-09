using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using ServiceStack.Text;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.IBS.Impl
{
    public class BookingWebServiceClient : BaseService<WebOrder7Service>, IBookingWebServiceClient
    {

        private const int _invalidZoneErrorCode = -1002;
        private IStaticDataWebServiceClient _staticDataWebServiceClient;
        private WebDriverService _webDriverService;

        public BookingWebServiceClient(IConfigurationManager configManager, ILogger logger, IStaticDataWebServiceClient staticDataWebServiceClient, WebDriverService webDriverService)
            : base(configManager, logger)
        {
            _staticDataWebServiceClient = staticDataWebServiceClient;
            _webDriverService = webDriverService;

        }

        protected override string GetUrl()
        {
            return base.GetUrl() + "IWEBOrder_7";
        }
        
        public IBSOrderStauts GetOrderStatus(int orderId, int accountId, string contactPhone)
        {
            var status = new IBSOrderStauts { Status = TWEBOrderStatusValue.wosNone.ToString() };
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

        public IBSDriverInfos GetDriverInfos(string driverId)
        {
            var infos = new IBSDriverInfos();
            var webDriver = _webDriverService.GetWEBDriver(UserNameApp, PasswordApp, driverId);
            infos.FirstName = webDriver.FirstName;
            infos.LastName = webDriver.LastName;
            infos.MobilePhone = webDriver.MobilePhone;
            infos.VehicleColor = webDriver.VehicleColor;
            infos.VehicleMake = webDriver.VehicleMake;
            infos.VehicleModel = webDriver.VehicleModel;
            infos.VehicleRegistration = webDriver.VehicleRegistration;
            infos.VehicleType = webDriver.VehicleType;
            return infos;
        }

        public IEnumerable<IBSOrderInformation> GetOrdersStatus(IList<int> ibsOrdersIds)
        {
            var result = new List<IBSOrderInformation>();
            UseService(service =>
                {
                    var status = service.GetOrdersStatus(UserNameApp, PasswordApp, ibsOrdersIds.ToArray());
                    foreach (var orderInfoFromIBS in status)
                    {
                        var statusInfos = new IBSOrderInformation();
                        statusInfos.Status = orderInfoFromIBS.OrderStatus.ToString();
                        statusInfos.IBSOrderId = orderInfoFromIBS.OrderID;
                        statusInfos.VehicleNumber = orderInfoFromIBS.VehicleNumber;
                        statusInfos.MobilePhone = orderInfoFromIBS.DriverMobilePhone;
                        statusInfos.FirstName = orderInfoFromIBS.DriverFirstName;
                        statusInfos.LastName = orderInfoFromIBS.DriverLastName;
                        statusInfos.VehicleColor = orderInfoFromIBS.VehicleColor;
                        statusInfos.VehicleLatitude = orderInfoFromIBS.VehicleCoordinateLat > 0 ? (double?)orderInfoFromIBS.VehicleCoordinateLat : null;
                        statusInfos.VehicleLongitude = orderInfoFromIBS.VehicleCoordinateLong > 0 ? (double?)orderInfoFromIBS.VehicleCoordinateLong : null;
                        statusInfos.VehicleMake = orderInfoFromIBS.VehicleMake;
                        statusInfos.VehicleModel = orderInfoFromIBS.VehicleModel;
                        statusInfos.VehicleRegistration = orderInfoFromIBS.VehicleRegistration;
                        statusInfos.Fare = orderInfoFromIBS.Fare;
                        statusInfos.Tip = orderInfoFromIBS.Tips;
                        statusInfos.Toll = orderInfoFromIBS.Tolls;
                        result.Add(statusInfos);
                    }
                });

            return result;
        }

        public int? CreateOrder(int? providerId, int accountId, string passengerName, string phone, int nbPassengers, int vehicleTypeId, int chargeTypeId, string note, DateTime pickupDateTime, IBSAddress pickup, IBSAddress dropoff)
        {
            Logger.LogMessage("WebService Create Order call : accountID=" + accountId);
            var order = new TBookOrder_5();

            order.ServiceProviderID = providerId.HasValue ? providerId.Value : 0;
            order.AccountID = accountId;
            order.Customer = passengerName;
            order.Phone = phone;

            var autoDispatch = ConfigManager.GetSetting("IBS.AutoDispatch").SelectOrDefault( setting => bool.Parse( setting ) , true );
            order.DispByAuto = autoDispatch;
                       

            order.PickupDate = new TWEBTimeStamp { Year = pickupDateTime.Year, Month = pickupDateTime.Month, Day = pickupDateTime.Day };
            order.PickupTime = new TWEBTimeStamp { Hour = pickupDateTime.Hour, Minute = pickupDateTime.Minute, Second = 0, Fractions = 0 };

            order.ChargeTypeID = chargeTypeId;
            var aptRing = Params.Get(pickup.Apartment, pickup.RingCode).Where(s => s.HasValue()).JoinBy(" / ");

            order.PickupAddress = new TWEBAddress { StreetPlace = pickup.FullAddress, AptBaz = aptRing, Longitude = pickup.Longitude, Latitude = pickup.Latitude };
            order.DropoffAddress = dropoff == null ? new TWEBAddress() : new TWEBAddress { StreetPlace = dropoff.FullAddress, AptBaz = dropoff.Apartment, Longitude = dropoff.Longitude, Latitude = dropoff.Latitude };
            order.Passengers = nbPassengers;
            order.VehicleTypeID = vehicleTypeId;
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
                Logger.LogMessage("WebService Creating IBS Order : " +  JsonSerializer.SerializeToString( order, typeof( TBookOrder_5 ) ) );
                Logger.LogMessage("WebService Creating IBS Order pickup : " + JsonSerializer.SerializeToString(order.PickupAddress, typeof(TWEBAddress)));
                Logger.LogMessage("WebService Creating IBS Order dest : " + JsonSerializer.SerializeToString(order.DropoffAddress, typeof(TWEBAddress)));


                orderId = service.SaveBookOrder_5(UserNameApp, PasswordApp, order);                
                Logger.LogMessage("WebService Create Order, orderid receveid : " + orderId);
            });
            return orderId;
        }

        private bool ValidateZoneAddresses(TBookOrder_5 order)
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
                var result = service.CancelBookOrder(UserNameApp, PasswordApp, orderId, contactPhone, null, accountId);
                isCompleted = result == 0;
            });
            return isCompleted;
        }
    }
}