using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [AuthorizationRequired(ApplyTo.All, Permissions.Admin)]
    [Route("/admin/addresses", "GET")]
    [Route("/admin/addresses", "POST")]
    [Route("/admin/addresses/{Id}", "PUT, DELETE")]
    public class DefaultFavoriteAddress : BaseDTO
    {
        public Guid Id { get; set; }

        public string FriendlyName { get; set; }

        public string FullAddress { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string Apartment { get; set; }

        public string RingCode { get; set; }

        public string BuildingName { get; set; }
    }

    [NoCache]
    public class DefaultFavoriteAddressResponse : List<DefaultAddressDetails>
    {
        public DefaultFavoriteAddressResponse()
        {
            
        }

        public DefaultFavoriteAddressResponse(IEnumerable<DefaultAddressDetails> collection)
            :base(collection)
        {
            
        }

    }
}
