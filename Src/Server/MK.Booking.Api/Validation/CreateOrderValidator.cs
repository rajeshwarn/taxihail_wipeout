#region

using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
//using ServiceStack.FluentValidation;

#endregion

namespace apcurium.MK.Booking.Api.Validation
{
    //TODO MKTAXI-3915: Handle this
    public class CreateOrderValidator// : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderValidator()
        {
            //Validation rules for all requests
            //RuleFor(r => r.Settings)
            //    .NotNull()
            //    .WithErrorCode(ErrorCode.CreateOrder_SettingsRequired.ToString());
        }
    }
}