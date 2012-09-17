using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Globalization;

namespace apcurium.MK.Booking.Api.Client
{
    public class PlaceDetailServiceClient: BaseServiceClient
    {
        public PlaceDetailServiceClient(string url, string sessionId)
            : base(url, sessionId)
        {
        }

         
        public Address GetPlaceDetail(string reference)
        {
            var result = Client.Get<Address>(string.Format(CultureInfo.InvariantCulture, "/places/{0}", reference));
            return result;
        }
    }
}
