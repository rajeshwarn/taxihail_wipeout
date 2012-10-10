using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Validation
{
    public class SaveFavoriteAddressValidator : AbstractValidator<SaveAddress>
    {
        public SaveFavoriteAddressValidator()
        {
            //Validation rules for all requests

            //Validation rules for POST and PUT request
            RuleSet(ApplyTo.Post | ApplyTo.Put, () =>
            {
                RuleFor(r => r.FriendlyName).NotEmpty();
                RuleFor(r => r.FullAddress).NotEmpty();
                RuleFor(r => r.Latitude).InclusiveBetween(-90d, 90d);
                RuleFor(r => r.Longitude).InclusiveBetween(-180d, 180d);
            });
        }
    }
}
