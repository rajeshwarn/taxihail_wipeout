using System;

namespace apcurium.MK.Booking.Api.Contract.Validation
{
    public class ValidationAttribute: Attribute
    {
        public string ErrorMessage { get; set; }

        public virtual bool IsValid(object value)
        {
            return true;
        }
    }
}
