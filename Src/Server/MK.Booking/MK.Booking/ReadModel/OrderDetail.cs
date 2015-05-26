﻿#region

using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.ReadModel
{
    public class OrderDetail
    {
        public OrderDetail()
        {
            //required by EF
            Settings = new BookingSettings();
            PaymentInformation = new PaymentInformationDetails();
            PickupAddress = new Address();
            DropOffAddress = new Address();
        }

        [Key]
        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        public DateTime PickupDate { get; set; }

        /// <summary>
        /// This date is saved in UTC
        /// </summary>
        public DateTime? DropOffDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? IBSOrderId { get; set; }

        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public BookingSettings Settings { get; set; }

        public PaymentInformationDetails PaymentInformation { get; set; }

        public int Status { get; set; }

        public double? Fare { get; set; }

        public double? Toll { get; set; }

        public double? Tip { get; set; }

        public double? Tax { get; set; }

        public double? Surcharge { get; set; }

        public decimal BookingFees { get; set; }

        public bool IsRemovedFromHistory { get; set; }

        public long TransactionId { get; set; }

        public bool IsRated { get; set; }

        public double? EstimatedFare { get; set; }

        public string UserNote { get; set; }

        public string CompanyKey { get; set; }

        public string CompanyName { get; set; }

        public string Market { get; set; }

        public string UserAgent { get; set; }

        public string ClientLanguageCode { get; set; }

        public string ClientVersion { get; set; }

        public bool IsManualRideLinq { get; set; }
    }
}