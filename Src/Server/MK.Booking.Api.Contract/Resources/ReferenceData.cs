using System.Collections.Generic;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class ReferenceData : BaseDTO
    {
        public IList<ListItem> CompaniesList { get; set; }
        public IList<ListItem> VehiclesList { get; set; }
        public IList<ListItem> PaymentsList { get; set; }


    }
}
