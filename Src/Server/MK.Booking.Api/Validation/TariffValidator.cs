#region

using apcurium.MK.Booking.Api.Contract.Requests;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Validation
{
    public class TariffValidator : AbstractValidator<Tariff>
    {
        public TariffValidator()
        {
            RuleSet(ApplyTo.Post | ApplyTo.Put, () =>
            {
                RuleFor(r => r.Name).NotEmpty();
                RuleFor(r => r.DaysOfTheWeek);
                RuleFor(r => r.FlatRate).GreaterThanOrEqualTo(0);
                RuleFor(r => r.KilometricRate).GreaterThanOrEqualTo(0);
                RuleFor(r => r.MarginOfError).GreaterThanOrEqualTo(0);
                RuleFor(r => r.KilometricRate).GreaterThanOrEqualTo(0);
                RuleFor(r => r.PassengerRate).GreaterThanOrEqualTo(0);
            });
        }
    }
}