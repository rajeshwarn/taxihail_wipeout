#region

using System;
using System.Collections;
using System.Collections.Generic;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class OrderCreated : VersionedEvent
    {
        public OrderCreated()
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

        public double? EstimatedFare { get; set; }

        public string UserAgent { get; set; }

        public string ClientLanguageCode { get; set; }

        public double? UserLatitude { get; set; }

        public double? UserLongitude { get; set; }

        public string UserNote { get; set; }

        public string ClientVersion { get; set; }

        public bool IsChargeAccountPaymentWithCardOnFile { get; set; }

        public string CompanyKey { get; set; }

        public string CompanyName { get; set; }

        public string Market { get; set; }

        public bool IsPrepaid { get; set; }
        
        public decimal BookingFees { get; set; }

        public double? TipIncentive { get; set; }

        /// <summary>
        /// Contains User Note and other informations
        /// </summary>
        public string IbsInformationNote { get; set; }

        public Fare Fare { get; set; }

        public int IbsAccountId { get; set; }

        public string[] Prompts { get; set; }

        public int?[] PromptsLength { get; set; }

        public Guid? PromotionId { get; set; }

        public bool IsFutureBooking { get; set; }

        public ListItem[] ReferenceDataCompanyList { get; set; }

        public string ChargeTypeEmail { get; set; }
    }
}