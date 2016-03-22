#if CLIENT
using ValidationAttribute = apcurium.MK.Booking.Api.Contract.Validation.ValidationAttribute;
#else
using ValidationAttribute = System.ComponentModel.DataAnnotations.ValidationAttribute;
#endif
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Contract.Validation
{
    public class TariffRatesValidator : ValidationAttribute
    {
        public TariffRatesValidator()
        {
            ErrorMessage = "GreaterThanOrEqual";
        }

        public override bool IsValid(object value)
        {
            var tariff = value as Tariff;

            if (tariff == null)
            {
                return false;
            }
            return tariff.FlatRate >= 0 &&
                tariff.KilometricRate >= 0 &&
                tariff.MarginOfError >= 0 &&
                tariff.PerMinuteRate >= 0;
        }
    }
}
