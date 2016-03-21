using System.ComponentModel.DataAnnotations;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Contract.Validation
{
    public class BookingSettingsValidation : ValidationAttribute
    {
        public BookingSettingsValidation()
        {
            ErrorMessage = ErrorCode.CreateOrder_SettingsRequired.ToString();
        }

        public override bool IsValid(object value)
        {
            return value != null;
        }
    }
}
