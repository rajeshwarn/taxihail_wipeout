#region

using apcurium.MK.Booking.Api.Contract.Requests;
//using ServiceStack.FluentValidation;

#endregion

namespace apcurium.MK.Booking.Api.Validation
{
    //TODO MKTAXI-3915: Handle this
    public class AvailableVehiclesValidator //: AbstractValidator<AvailableVehicles>
    {
        public AvailableVehiclesValidator()
        {
            //RuleFor(r => r.Latitude).InclusiveBetween(-90d, 90d);
            //RuleFor(r => r.Longitude).InclusiveBetween(-180d, 180d);
        }
    }
}