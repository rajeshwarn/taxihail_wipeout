using System;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class IbsOrderSwitchInitiated : VersionedEvent
    {
        public IbsOrderSwitchInitiated()
        {
            Settings = new BookingSettings();
        }

        public Guid AccountId { get; set; }

        public int? IBSOrderId { get; set; }

        public DateTime PickupDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public BookingSettings Settings { get; set; }

        public string ClientLanguageCode { get; set; }

        public bool IsChargeAccountPaymentWithCardOnFile { get; set; }

        public string CompanyKey { get; set; }

        public string CompanyName { get; set; }

        public string Market { get; set; }

        public bool IsPrepaid { get; set; }

        public double? TipIncentive { get; set; }

        /// <summary>
        /// Contains User Note and other informations
        /// </summary>
        public string IbsInformationNote { get; set; }

        public Fare Fare { get; set; }

        public int IbsAccountId { get; set; }

        public ListItem[] ReferenceDataCompanyList { get; set; }
    }
}
