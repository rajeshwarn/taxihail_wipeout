using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/accounts/{AccountId}/addresses", "POST")]
    [RestService("/accounts/{AccountId}/addresses/{Id}", "PUT, DELETE")]
    public class SaveAddress
    {
        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        public string FriendlyName { get; set; }

        public string FullAddress { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string Apartment { get; set; }

        public string RingCode { get; set; }

        public bool IsHistoric { get; set; }

    }
}
