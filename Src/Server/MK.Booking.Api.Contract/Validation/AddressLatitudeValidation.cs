#if CLIENT
using ValidationAttribute = apcurium.MK.Booking.Api.Contract.Validation.ValidationAttribute;
#else
using ValidationAttribute = System.ComponentModel.DataAnnotations.ValidationAttribute;
#endif
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Validation
{
    public class AddressLatitudeValidationAttribute : ValidationAttribute
    {
        
        public double MinLatitude { get; set; }
        public double MaxLatitude { get; set; }

        public AddressLatitudeValidationAttribute()
        {
            ErrorMessage = "InclusiveBetween";
        }

        public override bool IsValid(object value)
        {
            var address = value as Address;
            if (address == null)
            {
                return false;
            }

            return address.Latitude >= MinLatitude &&
                   address.Latitude <= MaxLatitude;
        }
    }
}
