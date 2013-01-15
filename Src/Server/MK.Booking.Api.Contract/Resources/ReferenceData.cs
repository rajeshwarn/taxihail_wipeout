using System.Collections.Generic;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class ReferenceData : BaseDTO
    {
        public const int CreditCardOnFileType = 666;

        public IList<NullableListItem> CompaniesList { get; set; }
        public IList<NullableListItem> VehiclesList { get; set; }
        public IList<ListItem> PaymentsList { get; set; }

        public IList<ListItem> PickupCityList { get; set; }
        public IList<ListItem> DropoffCityList { get; set; }

    }
}
