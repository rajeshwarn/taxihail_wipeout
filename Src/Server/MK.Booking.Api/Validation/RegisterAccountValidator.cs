using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Validation
{
    public class RegisterAccountValidator : AbstractValidator<RegisterAccount>
    {
        public RegisterAccountValidator()
        {
            RuleSet(ApplyTo.Post,() =>
            {
                RuleFor(x => x.Email).NotNull();
                RuleFor(x => x.Name).NotNull();
            });
        }
    }
}
