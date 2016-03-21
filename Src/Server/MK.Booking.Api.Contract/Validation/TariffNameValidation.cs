using System.ComponentModel.DataAnnotations;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Contract.Validation
{
    public class TariffNameValidation : ValidationAttribute
    {
        public TariffNameValidation()
        {
            ErrorMessage = "NotEmpty";
        }

        public override bool IsValid(object value)
        {
            var tariff = value as Tariff;
            return tariff != null && tariff.Name.HasValueTrimmed();
        }
    }
}
