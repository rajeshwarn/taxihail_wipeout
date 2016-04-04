#region

using System;
using apcurium.MK.Booking.Api.Contract.Validation;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/accounts/addresses", "POST")]
    [Route("/accounts/addresses/{Id}", "PUT, DELETE")]
    public class SaveAddress
    {
        public Guid Id { get; set; }
        [AddressNameValidation]
        [AddressLatitudeValidation(MinLatitude = -90d, MaxLatitude = 90d),
         AddressLongitudeValidation(MinLongitude = -180d, MaxLongitude = 180d)]
        public Address Address { get; set; }
    }
}