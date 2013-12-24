#region

using System.Collections.Generic;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class ReferenceData : BaseDto
    {
        public IList<ListItem> CompaniesList { get; set; }
        public IList<ListItem> VehiclesList { get; set; }
        public IList<ListItem> PaymentsList { get; set; }
    }
}