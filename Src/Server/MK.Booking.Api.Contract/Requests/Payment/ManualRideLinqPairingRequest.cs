using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    public class ManualRideLinqPairingRequest : IReturn<Order>
    {
        public string PairingCode { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public string ClientLanguageCode { get; set; }
    }
}
