using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Data
{
    public class TemporaryOrderCreationInfo
    {
        public Guid OrderId { get; set; }

        public Guid AccountId { get; set; }

        public CreateOrder Request { get; set; }

        public IList<ListItem> ReferenceDataCompaniesList { get; set; }

        public string ChargeTypeIbs { get; set; }

        public string VehicleType { get; set; }

        public string[] Prompts { get; set; }

        public int?[] PromptsLength { get; set; }

        public BestAvailableCompany BestAvailableCompany { get; set; }

        public Guid? PromotionId { get; set; }
    }
}
