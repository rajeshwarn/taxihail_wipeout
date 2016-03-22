#if CLIENT
using ValidationAttribute = apcurium.MK.Booking.Api.Contract.Validation.ValidationAttribute;
#else
using ValidationAttribute = System.ComponentModel.DataAnnotations.ValidationAttribute;
#endif
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Validation
{
    public class AddressLongitudeValidationAttribute: ValidationAttribute
    {
        public double MinLongitude { get; set; }
        public double MaxLongitude { get; set; }

        public AddressLongitudeValidationAttribute()
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

            return address.Longitude >= MinLongitude &&
                   address.Longitude <= MaxLongitude;
        }
    }
}
