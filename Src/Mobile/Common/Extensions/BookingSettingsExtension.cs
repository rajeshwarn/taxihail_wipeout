using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class BookingSettingsExtension
    {
        public static BookingSettings Copy(this BookingSettings instance)
        {
            var copy = new BookingSettings
            {
                Name = instance.Name,
                Phone = instance.Phone,
                VehicleTypeId = instance.VehicleTypeId,
                ChargeTypeId = instance.ChargeTypeId,
                ProviderId = instance.ProviderId,
                NumberOfTaxi = instance.NumberOfTaxi
            };
            return copy;


        }
    }
}