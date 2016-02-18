using apcurium.MK.Booking.Api.Contract.Requests;
//using ServiceStack.FluentValidation;
//using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Validation
{
    //TODO MKTAXI-3915: Handle this
    public class PopularAddressValidator //: AbstractValidator<PopularAddress>
    {
        public PopularAddressValidator()
        {
            //Validation rules for POST and PUT request
            //RuleSet(ApplyTo.Post | ApplyTo.Put, () =>
            //{
            //    RuleFor(r => r.Address.Latitude).InclusiveBetween(-90d, 90d);
            //    RuleFor(r => r.Address.Longitude).InclusiveBetween(-180d, 180d);
            //});
        }
    }
}