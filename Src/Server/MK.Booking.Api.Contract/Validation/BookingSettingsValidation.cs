#if CLIENT
using ValidationAttribute = apcurium.MK.Booking.Api.Contract.Validation.ValidationAttribute;
#else
using ValidationAttribute = System.ComponentModel.DataAnnotations.ValidationAttribute;
#endif
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
