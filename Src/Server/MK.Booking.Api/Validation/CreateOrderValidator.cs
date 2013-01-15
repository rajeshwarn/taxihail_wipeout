using ServiceStack.FluentValidation;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Validation
{
    public class CreateOrderValidator: AbstractValidator<CreateOrder>
    {
        public CreateOrderValidator()
        {
            //Validation rules for all requests
            RuleFor(r => r.Settings)
                .NotNull()
                .WithErrorCode(ErrorCode.CreateOrder_SettingsRequired.ToString());
        }
    }
}
