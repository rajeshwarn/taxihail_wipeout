using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Validation
{
    public class RatesValidator : AbstractValidator<Rates>
    {
        public RatesValidator()
        {
            RuleSet(ApplyTo.Post | ApplyTo.Put, () =>
            {
                RuleFor(r => r.Name).NotEmpty();
                RuleFor(r => r.DaysOfTheWeek).Must(x=>x > 0);
                RuleFor(r => r.FlatRate).GreaterThanOrEqualTo(0);
                RuleFor(r => r.DistanceMultiplicator).GreaterThanOrEqualTo(0);
                RuleFor(r => r.TimeAdjustmentFactor).GreaterThanOrEqualTo(0);
                RuleFor(r => r.PricePerPassenger).GreaterThanOrEqualTo(0);
                RuleFor(r => r.EndTime).GreaterThan(x => x.StartTime);
            });
        }
    }
}
