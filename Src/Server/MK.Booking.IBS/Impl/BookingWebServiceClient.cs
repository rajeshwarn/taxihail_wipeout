using System;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.IBS.Impl
{
    public class BookingWebServiceClient : BaseService<WebOrder7Service>, IBookingWebServiceClient
    {
        protected override string GetUrl()
        {
            return base.GetUrl() + "IWEBOrder_7";
        }
        

        public BookingWebServiceClient(IConfigurationManager configManager, ILogger logger)
            : base(configManager, logger)
        {
           
        }

        public IBSOrderStauts GetOrderStatus(int orderId, int accountId)
        {
            var status = new IBSOrderStauts { Status = TWEBOrderStatusValue.wosNone.ToString() };
            UseService(service =>
            {
                var orderStatus = service.GetOrderStatus(UserNameApp, PasswordApp, orderId, string.Empty, string.Empty, accountId);
                status.Status = orderStatus.ToString();

                double latitude = 0;
                double longitude = 0;
                var result = service.GetVehicleLocation(UserNameApp, PasswordApp, orderId, ref latitude, ref longitude);

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
                var order = service.GetBookOrder_5(UserNameApp, PasswordApp, orderId, contactPhone, null, accountId);
                if (order != null)
                {
                    result.CabNumber = order.CabNo.ToSafeString().Trim();
                    result.Fare = order.Fare == 0 ? (double?)null : order.Fare;
                    result.Toll = order.Tolls == 0 ? (double?)null : order.Tolls;
                }
            });
            return result;
        }

        public int? CreateOrder(int providerId, int accountId, string passengerName, string phone, int nbPassengers, int vehicleTypeId, string note, DateTime pickupDateTime, IBSAddress pickup, IBSAddress dropoff)
        {
            Logger.LogMessage("WebService Create Order call : accountID=" + accountId);
            var order = new TBookOrder_5();

            order.ServiceProviderID = providerId;
            order.AccountID = accountId;
            order.Customer = passengerName;
            order.Phone = phone;


            order.PickupDate = new TWEBTimeStamp { Year = pickupDateTime.Year, Month = pickupDateTime.Month, Day = pickupDateTime.Day };
            order.PickupTime = new TWEBTimeStamp { Hour = pickupDateTime.Hour, Minute = pickupDateTime.Minute, Second = 0, Fractions = 0 };

            order.PickupAddress = new TWEBAddress { StreetPlace = pickup.FullAddress, AptBaz = pickup.Apartment, Longitude = pickup.Longitude, Latitude = pickup.Latitude };
            order.DropoffAddress = dropoff == null ? new TWEBAddress() : new TWEBAddress { StreetPlace = pickup.FullAddress, AptBaz = pickup.Apartment, Longitude = pickup.Longitude, Latitude = pickup.Latitude };
            order.Passengers = nbPassengers;
            order.VehicleTypeID = vehicleTypeId;
            order.Note = note;
            order.ContactPhone = phone;
            order.OrderStatus = TWEBOrderStatusValue.wosPost;

            int? orderId = null;


            UseService(service =>
            {
                 orderId = service.SaveBookOrder_5(UserNameApp, PasswordApp, order);
                 Logger.LogMessage("WebService Create Order, orderid receveid : " + orderId);
            });
            return orderId;
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