#region

using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderMapper
    {
        public Order ToResource(OrderDetail order)
        {
            var resource = new Order
            {
                Id = order.Id,
                PickupDate = order.PickupDate,
                CreatedDate = order.CreatedDate,
                IBSOrderId = order.IBSOrderId,
                PickupAddress = order.PickupAddress,
                DropOffAddress = order.DropOffAddress,
                Settings = new BookingSettings
                {
                    ChargeTypeId = order.Settings.ChargeTypeId,
                    Name = order.Settings.Name,
                    Phone = order.Settings.Phone,
                    NumberOfTaxi = 1,
                    Passengers = order.Settings.Passengers,
                    ProviderId = order.Settings.ProviderId,
                    VehicleTypeId = order.Settings.VehicleTypeId,
                    VehicleType = order.Settings.VehicleType,
                    LuxuryVehicleTypeId = order.Settings.LuxuryVehicleTypeId,
                    LuxuryVehicleType = order.Settings.LuxuryVehicleType,
                    LargeBags = order.Settings.LargeBags,
                    AccountNumber = order.Settings.AccountNumber,
                    CustomerNumber = order.Settings.CustomerNumber,
                    ServiceType = order.Settings.ServiceType,
                    Country = order.Settings.Country,
                    PayBack = order.Settings.PayBack
                },
                Note = order.UserNote,
                Fare = order.Fare,
                Tax = order.Tax,
                Tip = order.Tip,
                Toll = order.Toll,
                Surcharge = order.Surcharge,
                Extra = order.Extra,
                Status = (OrderStatus) order.Status,
                IsRated = order.IsRated,
                Gratuity = order.Gratuity,
                TransactionId = order.TransactionId,
                IsManualRideLinq = order.IsManualRideLinq,
            };

            return resource;
        }

    }
}