#region

using apcurium.MK.Booking.Api.Contract.Requests;
//using ServiceStack.FluentValidation;
//using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Validation
{
    //TODO MKTAXI-3915: Handle this
    public class RegisterAccountValidator// : AbstractValidator<RegisterAccount>
    {
        public RegisterAccountValidator()
        {
            //RuleSet(ApplyTo.Post, () =>
            //{
            //    RuleFor(x => x.Email).NotNull();
            //    RuleFor(x => x.Name).NotNull();
            //    RuleFor(x => x.Country.Code).NotNull();
            //});
        }
    }
}