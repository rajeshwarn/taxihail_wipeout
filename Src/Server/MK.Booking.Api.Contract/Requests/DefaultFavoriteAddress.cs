#region

using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Validation;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/admin/addresses", "GET")]
    [RouteDescription("/admin/addresses", "POST")]
    [RouteDescription("/admin/addresses/{Id}", "PUT, DELETE")]
    public class DefaultFavoriteAddress : BaseDto
    {
        public Guid Id { get; set; }

        [AddressLatitudeValidation(MinLatitude = -90d, MaxLatitude = 90d),
         AddressLongitudeValidation(MinLongitude = -180d, MaxLongitude = 180d)]
        public Address Address { get; set; }
    }

    public class DefaultFavoriteAddressResponse : List<DefaultAddressDetails>
    {
        public DefaultFavoriteAddressResponse()
        {
        }

        public DefaultFavoriteAddressResponse(IEnumerable<DefaultAddressDetails> collection)
            : base(collection)
        {
        }
    }
}