using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Entity;

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

        public bool IsRemovedFromHistory { get; set; }

        public long TransactionId { get; set; }

        public bool IsRated { get; set; }

        public double? EstimatedFare { get; set; }

        public string UserAgent  { get; set; }
    }
}
