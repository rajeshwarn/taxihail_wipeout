#if CLIENT
using ValidationAttribute = apcurium.MK.Booking.Api.Contract.Validation.ValidationAttribute;
#else
using ValidationAttribute = System.ComponentModel.DataAnnotations.ValidationAttribute;
#endif
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Contract.Validation
{
    public class AddressNameValidationAttribute : ValidationAttribute
    {

        public AddressNameValidationAttribute()
        {
            ErrorMessage = "NotEmpty";
        }

        public override bool IsValid(object value)
        {
            var address = value as Address;
            if (address == null)
            {
                return false;
            }
            
            return address.FullAddress.HasValueTrimmed() && address.FriendlyName.HasValueTrimmed();
        }
    }
}
